using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Domain.Statistics;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL;

public class StatisticsManager : IProjectStatisticsManager
{
    private readonly IProjectRepository _projectRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly IIdeaRepository _ideaRepository;
    private readonly IAiRepository _aiRepository;

    public StatisticsManager(
        IProjectRepository projectRepository,
        IQuestionRepository questionRepository,
        ISurveyRepository surveyRepository,
        IIdeaRepository ideaRepository,
        IAiRepository aiRepository)
    {
        _projectRepository = projectRepository;
        _questionRepository = questionRepository;
        _surveyRepository = surveyRepository;
        _ideaRepository = ideaRepository;
        _aiRepository = aiRepository;
    }

    public ProjectStatistics GetProjectStatistics(int projectId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("ProjectId moet groter zijn dan 0.", nameof(projectId));
        }

        var project = _projectRepository.ReadProject(projectId);

        if (project == null)
        {
            throw new KeyNotFoundException($"Project met ID {projectId} werd niet gevonden.");
        }

        var questionList = _questionRepository.ReadQuestionListByProject(project);

        var surveyResponses = _surveyRepository
            .ReadSurveyResponsesByProjectId(projectId)
            .ToList();

        var ideas = _ideaRepository
            .ReadAcceptedIdeasWithReactionsByProjectId(projectId)
            .ToList();

        var statistics = new ProjectStatistics
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ParticipantsCount = surveyResponses
                .Select(sr => sr.UserId)
                .Distinct()
                .Count(),
            IdeasCount = ideas.Count,
            ReactionsCount = ideas.Sum(i => i.Reactions?.Count() ?? 0)
        };

        if (questionList == null)
        {
            return statistics;
        }

        var questions = questionList.Sections
            .OrderBy(s => s.Order)
            .SelectMany(s => s.Questions)
            .ToList();

        foreach (var question in questions)
        {
            var answers = surveyResponses
                .SelectMany(sr => sr.Answers)
                .Where(a => a.QuestionId == question.Id)
                .ToList();

            var questionStats = new QuestionStatistics
            {
                QuestionId = question.Id,
                Question = question.Description,
                QuestionType = question.QuestionType.ToString(),
                TotalAnswers = answers.Count
            };
            if (question.QuestionType == QuestionType.OpenQuestion)
            {
                var openAnswers = answers
                    .Where(a => !string.IsNullOrWhiteSpace(a.Text))
                    .OrderBy(a => a.Id)
                    .ToList();

                questionStats.OpenAnswers = openAnswers
                    .Select(a => a.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                var existingSummary = _aiRepository.ReadOpenQuestionSummary(projectId, question.Id);

                questionStats.AiSummary = existingSummary?.Summary;
                questionStats.AiSummaryGeneratedAt = existingSummary?.GeneratedAt;

                if (existingSummary != null)
                {
                    var currentAnswerCount = openAnswers.Count;
                    var currentLastAnswerId = openAnswers.Any()
                        ? openAnswers.Max(a => a.Id)
                        : 0;

                    questionStats.AiSummaryNeedsRefresh =
                        existingSummary.AnswerCountAtGeneration != currentAnswerCount ||
                        existingSummary.LastAnswerIdAtGeneration != currentLastAnswerId;
                }
            }

            if (question.QuestionType == QuestionType.Range)
            {
                var numericAnswers = answers
                    .Select(a => int.TryParse(a.Text, out var value) ? value : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
                    .ToList();

                if (numericAnswers.Any())
                {
                    questionStats.Average = Math.Round(numericAnswers.Average(), 1);
                }

                questionStats.Options = numericAnswers
                    .GroupBy(v => v.ToString())
                    .Select(g => new AnswerOptionStatistics
                    {
                        Answer = g.Key,
                        Count = g.Count(),
                        Percentage = CalculatePercentage(g.Count(), numericAnswers.Count)
                    })
                    .OrderBy(o => int.Parse(o.Answer))
                    .ToList();
            }
            else if (question.QuestionType == QuestionType.SingleChoice ||
                     question.QuestionType == QuestionType.MultipleChoice)
            {
                questionStats.Options = answers
                    .SelectMany(a => SplitAnswer(a.Text))
                    .GroupBy(a => a)
                    .Select(g => new AnswerOptionStatistics
                    {
                        Answer = g.Key,
                        Count = g.Count(),
                        Percentage = CalculatePercentage(g.Count(), answers.Count)
                    })
                    .OrderByDescending(o => o.Count)
                    .ToList();
            }

            statistics.Questions.Add(questionStats);
        }

        return statistics;
    }

    private static List<string> SplitAnswer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            return new List<string>();
        }

        return answer
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static double CalculatePercentage(int count, int total)
    {
        if (total == 0)
        {
            return 0;
        }

        return Math.Round((double)count / total * 100, 1);
    }
}