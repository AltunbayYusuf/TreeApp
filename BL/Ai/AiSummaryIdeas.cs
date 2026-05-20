using System.Text;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.interfaces;
using IntegratieProject.DAL.Interfaces;
using Microsoft.Extensions.Options;

namespace IntegratieProject.BL.Ai;

public class AiSummaryIdeas : IAiSummaryIdeas
{
    private readonly IIdeaRepository _ideaRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly IAiPromptService _promptService;
    private readonly IAiProvider _aiProvider;
    private readonly IAiUsageRepository _aiUsageRepository;
    private readonly IAiRepository _aiRepository;
    private readonly AiModelSettings _aiModelSettings;
    
    private const int MaxIdeas = 40;
    private const int MaxReactionsPerIdea = 5;
    private const int MaxSurveyResponses = 50;
    private const int MaxAnswersPerSurveyResponse = 15;
    private const int MaxTextLength = 350;

    public AiSummaryIdeas(
        IIdeaRepository ideaRepository,
        ISurveyRepository surveyRepository,
        IAiPromptService promptService,
        IAiProvider aiProvider,
        IAiUsageRepository aiUsageRepository,
        IAiRepository aiRepository,
        IOptions<AiModelSettings> aiModelSettings)
    {
        _ideaRepository = ideaRepository;
        _surveyRepository = surveyRepository;
        _promptService = promptService;
        _aiProvider = aiProvider;
        _aiUsageRepository = aiUsageRepository;
        _aiRepository = aiRepository;
        _aiModelSettings = aiModelSettings.Value;
    }

    public async Task<string> GenerateProjectTrendSummaryAsync(int projectId)
    {
        try
        {
            var ideas = _ideaRepository
                .ReadAcceptedIdeasWithReactionsByProjectId(projectId)
                .Take(MaxIdeas)
                .ToList();

            var surveyResponses = _surveyRepository
                .ReadSurveyResponsesByProjectId(projectId)
                .Take(MaxSurveyResponses)
                .ToList();

            if (!ideas.Any() && !surveyResponses.Any())
                return "Er zijn nog geen goedgekeurde ideeën, reacties of ingevulde enquêtes om samen te vatten.";

            var projectData = BuildCompactProjectData(ideas, surveyResponses);
            var prompt = _promptService.BuildProjectTrendSummaryPrompt(projectData);

            var summary = await _aiProvider.GenerateAsync(prompt);

            _aiUsageRepository.AddUsage(new AiUsage
            {
                Feature = "ProjectTrendSummary",
                Model = _aiModelSettings.ModerationModel,
                Success = true
            });

            return summary;
        }
        catch (Exception ex)
        {
            _aiUsageRepository.AddUsage(new AiUsage
            {
                Feature = "ProjectTrendSummary",
                Model = _aiModelSettings.ModerationModel,
                Success = false,
                ErrorMessage = ex.Message
            });

            return "AI-samenvatting tijdelijk niet beschikbaar. Probeer later opnieuw.";
        }
    }

    private static string BuildCompactProjectData(
        IEnumerable<Idea> ideas,
        IEnumerable<SurveyResponse> surveyResponses)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== IDEEËN EN REACTIES ===");

        foreach (var idea in ideas)
        {
            sb.AppendLine($"Topic: {idea.Topic?.Theme}");
            sb.AppendLine($"Idee titel: {Limit(idea.Title)}");
            sb.AppendLine($"Idee tekst: {Limit(idea.Text)}");

            var reactions = idea.Reactions?
                .Where(r => !string.IsNullOrWhiteSpace(r.Text) || !string.IsNullOrWhiteSpace(r.Emoji))
                .Take(MaxReactionsPerIdea)
                .ToList() ?? new List<Reaction>();

            if (reactions.Any())
            {
                sb.AppendLine("Reacties:");

                foreach (var reaction in reactions)
                {
                    if (!string.IsNullOrWhiteSpace(reaction.Text))
                        sb.AppendLine($"- {Limit(reaction.Text)}");

                    if (!string.IsNullOrWhiteSpace(reaction.Emoji))
                        sb.AppendLine($"- Emoji: {reaction.Emoji}");
                }
            }

            sb.AppendLine();
        }

        sb.AppendLine("=== INGEVULDE ENQUÊTES ===");

        foreach (var response in surveyResponses)
        {
            sb.AppendLine($"Ingevuld op: {response.SubmittedAt:yyyy-MM-dd}");

            foreach (var answer in response.Answers.Take(MaxAnswersPerSurveyResponse))
            {
                sb.AppendLine($"Vraag: {Limit(answer.Question?.Description)}");
                sb.AppendLine($"Antwoord: {Limit(answer.Text)}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
    
    public async Task<string> GenerateOpenQuestionSummaryAsync(int projectId, int questionId)
    {
        var surveyResponses = _surveyRepository
            .ReadSurveyResponsesByProjectId(projectId)
            .ToList();

        var answers = surveyResponses
            .SelectMany(sr => sr.Answers)
            .Where(a => a.QuestionId == questionId)
            .Where(a => !string.IsNullOrWhiteSpace(a.Text))
            .OrderBy(a => a.Id)
            .ToList();

        if (!answers.Any())
        {
            return "Er zijn nog geen open antwoorden om samen te vatten.";
        }

        var answerCount = answers.Count;
        var lastAnswerId = answers.Max(a => a.Id);

        var existingSummary = _aiRepository.ReadOpenQuestionSummary(projectId, questionId);

        if (existingSummary != null &&
            existingSummary.AnswerCountAtGeneration == answerCount &&
            existingSummary.LastAnswerIdAtGeneration == lastAnswerId)
        {
            return existingSummary.Summary;
        }

        var questionText = answers.First().Question?.Description ?? "Open vraag";

        var answersText = string.Join("\n", answers.Select(a => $"- {Limit(a.Text)}"));

        var prompt = _promptService.BuildOpenQuestionSummaryPrompt(questionText, answersText);

        var summary = await _aiProvider.GenerateAsync(prompt);

        _aiRepository.SaveOpenQuestionSummary(
            projectId,
            questionId,
            summary,
            answerCount,
            lastAnswerId
        );

        _aiUsageRepository.AddUsage(new AiUsage
        {
            Feature = "OpenQuestionSummary",
            Model = _aiModelSettings.ModerationModel,
            Success = true
        });

        return summary;
    }

    private static string Limit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        value = value.Trim();

        return value.Length <= MaxTextLength
            ? value
            : value[..MaxTextLength] + "...";
    }
}