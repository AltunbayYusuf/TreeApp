using System.Text.Json;
using IntergratieProject.DAL;
using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.users;

namespace IntergratieProject.BL;

public class Manager : IManager
{
    private readonly IAiService _aiService;
    private readonly IIdeaRepository _repository;
    

    public Manager(IAiService aiService, IIdeaRepository repository)
    {
        _aiService = aiService;
        _repository = repository;
    }
    
    public async Task ForceAddReactionAsync(int ideaId, string? emoji, string? text, int? userId)
    {
        var idea = _repository.ReadIdeaById(ideaId);

        if (idea == null)
        {
            throw new Exception("Idee niet gevonden");
        }

        var reaction = new Reaction
        {
            UserId = userId,
            Idea = idea,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
            Text = string.IsNullOrWhiteSpace(text) ? null : text,
            ModerationStatus = ModerationStatus.InReview
        };

        _repository.AddReaction(reaction);
        await Task.CompletedTask;
    }

    public void SubmitIdeaFromChatAsync(int topicId, string text)
    {
        
    }

    public async Task<string> AskAiForIdea(string idea)
    {
        var prompt = $"Analyseer dit idee en geef feedback:\n{idea}";

        return await _aiService.GenerateAsync(prompt, FeatureType.IdeaSelection);
    }
    
    public async Task<ToxicityResult> ModerateTextAsync(string input)
{
    var prompt =
        """
        Je bent een moderatie-assistent voor een jongerenplatform.
        Antwoord ALLEEN met een JSON object. Geen markdown, geen code fences, geen extra tekst.

        JSON schema:
        {"isToxic":true/false,"explanation":"...","suggestedText":"..."}

        Regels:
        - isToxic=true bij schelden, haatspraak, bedreiging, intimidatie, vernedering
        - suggestedText: respectvolle herformulering met dezelfde bedoeling (leeg als niet toxisch)

        INPUT:
        """ + input;

    var aiText = await _aiService.GenerateAsync(prompt, FeatureType.Moderation);
    
    //debugging
    Console.WriteLine("RAW AI TEXT:");
    Console.WriteLine(aiText);
    Console.WriteLine("------");

    // strip ```json fences als die er zijn
    var cleaned = aiText.Trim();
    var firstBrace = cleaned.IndexOf('{');
    var lastBrace = cleaned.LastIndexOf('}');

    if (firstBrace >= 0 && lastBrace > firstBrace)
    {
        cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
    }

    try
    {
        using var doc = JsonDocument.Parse(cleaned);

        bool isToxic = doc.RootElement.GetProperty("isToxic").GetBoolean();
        string explanation = doc.RootElement.GetProperty("explanation").GetString() ?? "";
        string suggestedText = doc.RootElement.GetProperty("suggestedText").GetString() ?? "";

        return new ToxicityResult
        {
            IsToxic = isToxic,
            AiUnavailable = false,
            SuggestedText = suggestedText,
            Explanation = explanation
        };
    }
    catch (Exception ex)
    {
        // AI faalde / output niet parsebaar
        // return new ToxicityResult
        // {
        //     IsToxic = true,
        //     AiUnavailable = true,
        //     SuggestedText = "",
        //     Explanation = $"Moderation check failed: {ex.Message}. Raw: {aiText}"
        // };
            throw new Exception("AI moderation tijdelijk niet beschikbaar.", ex);

    }
}
    public async Task<ToxicityResult> AddReaction(int ideaId, string emoji, string text, int? userId)
    {
        var idea = _repository.ReadIdeaById(ideaId);

        if (idea == null)
        {
            throw new Exception("Idee niet gevonden");
        }

        if (!string.IsNullOrWhiteSpace(emoji) && string.IsNullOrWhiteSpace(text))
        {
            var reaction = new Reaction
            {
                UserId = userId,
                Idea = idea,
                Emoji = emoji,
                Text = null,
                ModerationStatus = ModerationStatus.Accepted
            };

            _repository.AddReaction(reaction);

            return new ToxicityResult
            {
                IsToxic = false,
                SuggestedText = "",
                AiUnavailable = false,
                Explanation = "Emoji reactie opgeslagen."
            };
        }

        // ALS ER TEKST IS → AI MODERATIE
        var moderation = await ModerateTextAsync(text);
        
        if (moderation.IsToxic)
        {
            return moderation;
        }

        var textReaction = new Reaction
        {
            UserId = userId,
            Idea = idea,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
            Text = text,
            ModerationStatus = ModerationStatus.Accepted
        };

        _repository.AddReaction(textReaction);

        return new ToxicityResult
        {
            IsToxic = false,
            SuggestedText = "",
            AiUnavailable = false,
            Explanation = "Reactie succesvol opgeslagen."
        };
    }
    

public async Task ForceSubmitIdeaAsync(int topicId, string title, string text)
{
    var topic = _repository.ReadTopicById(topicId);
    if (topic == null)
    {
        throw new Exception("Topic niet gevonden");
    }

    var idea = new Idea
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title,
        Text = text,
        Topic = topic,
        ModerationStatus = ModerationStatus.InReview
    };

    _repository.AddIdea(idea);
    await Task.CompletedTask;
} 
public async Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text)
    {
        var topic = _repository.ReadTopicById(topicId);
        if (topic == null)
        {
            throw new Exception("Topic niet gevonden");
        }

        var moderation = await ModerateTextAsync(text);
        
        if (moderation.IsToxic)
        {
            return moderation;
        }

        var idea = new Idea
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title,
            Text = text,
            Topic = topic,
            ModerationStatus = ModerationStatus.Accepted
        };
        ValidateEntety(idea);

        _repository.AddIdea(idea);

        return new ToxicityResult
        {
            IsToxic = false,
            SuggestedText = "",
            Explanation = "Idee succesvol opgeslagen."
        };
    }
    public IEnumerable<Topic> GetTopicsByProject(Project project)
    {
        return _repository.ReadTopicsByProject(project);
    }

    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null)
    {
        IEnumerable<Idea> ideas;

        if (topicId.HasValue)
        {
            ideas = _repository.ReadIdeasByTopic(project, topicId.Value);
        }
        else
        {
            ideas = _repository.ReadIdeasByProject(project);
        }

        return ideas.Where(i => i.ModerationStatus == ModerationStatus.Accepted);
    }

    public IEnumerable<Question> GetAllQuestionsBySection(int sectionId)
    {
        return _repository.ReadAllQuestionsBySection(sectionId);
    }
    public IEnumerable<Question> GetAllQuestions()
    {
        return _repository.ReadAllQuestions();
    }

    public Question GetQuestion(int questionId)
    {
        return _repository.ReadQuestion(questionId);
    }

    public QuestionList GetQuestionListByProject(Project project)
    {
        return _repository.ReadQuestionListByProject(project);
    }

    public Project? GetProject(int projectId)
    {
        return _repository.ReadProject(projectId);
    }

    public User? GetUser(string cookieId)
    {
        return _repository.ReadUser(cookieId);
    }

    public void AddUser(User user)
    {
        ValidateEntety(user);
        _repository.CreateUser(user);
    }

    public SurveyResponse? GetSurveyResponse(int userId, int projectId)
    {
        return _repository.ReadSurveyResponse(userId, projectId);
    }

    public void SaveSurveyResponse(int userId, int projectId, List<Answer> answers)
    {
        _repository.SaveSurveyResponse(userId, projectId, answers);
    }
    
    public SubPlatform? GetSubPlatformBySlug(string slug)
    {
        return _repository.ReadSubPlatformBySlug(slug);
    }

    public Project? GetProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId)
    {
        return _repository.ReadProjectBySubPlatformAndProjectId(subplatformSlug, projectId);
    }

    public IEnumerable<Project> GetProjectsBySubPlatform(int subPlatformId)
    {
        return _repository.ReadProjectsBySubPlatform(subPlatformId);
    }
    
    public Project? GetFirstProjectBySubPlatform(string slug)
    {
        var subPlatform = _repository.ReadSubPlatformBySlug(slug);

        if (subPlatform == null)
        {
            return null;
        }

        return _repository.ReadProjectsBySubPlatform(subPlatform.Id)
            .OrderBy(p => p.Id).FirstOrDefault();
    }

    public void UpdateProject(Project project)
    {
        ValidateEntety(project);
        _repository.ChangeProject(project);
    }
    public void CreateProject(Project project)
    {
        ValidateEntety(project);
        _repository.CreateProject(project);
    }
    
    private void ValidateEntety(Object model)
    {
        var validationResults = new List<ValidationResult>();
        bool success = Validator.TryValidateObject(model,
            new ValidationContext(model), validationResults, true);

        if (!success)
        {
            var message = "";
            foreach (var validationResult in validationResults)
            {
                message += validationResult.ErrorMessage + " ";
            }

            throw new ValidationException(message);
        } 
    }
    
    
    public IEnumerable<Idea> GetIdeasInReviewBySubPlatform(int subPlatformId)
    {
        return _repository.ReadIdeasInReviewBySubPlatform(subPlatformId);
    }

    public IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId)
    {
        return _repository.ReadReactionsInReviewBySubPlatform(subPlatformId);
    }

   

}