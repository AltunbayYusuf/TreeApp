using System.Text.Json;
using IntegratieProject.BL.Ai;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class IdeaManager : IIdeaManager
{
    private readonly IIdeaRepository _ideaRepository;
    private readonly IReactionRepository _reactionRepository;
    private readonly IManager _manager;
    private readonly IAiModerationService _aiModerationService;
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _aiPromptService;
    private readonly ITopicRepository _topicRepository;
    private readonly AiUsageService _aiUsageService;
    private readonly IAiModelConfigurationManager _aiModelConfigurationManager;

    public IdeaManager(IIdeaRepository ideaRepository,
        IReactionRepository reactionRepository, IManager manager, IAiModerationService aiModerationService,
        IAiProvider aiProvider, IAiPromptService aiPromptService, ITopicRepository topicRepository,
        AiUsageService aiUsageService, IAiModelConfigurationManager modelConfigurationManager)
    {
        _reactionRepository = reactionRepository;
        _ideaRepository = ideaRepository;
        _manager = manager;
        _aiModerationService = aiModerationService;
        _aiProvider = aiProvider;
        _aiPromptService = aiPromptService;
        _topicRepository = topicRepository;
        _aiUsageService = aiUsageService;
        _aiModelConfigurationManager = modelConfigurationManager;
    }

    public async Task ForceSubmitIdeaAsync(int topicId, string title, string text, int? userId, string imageUri = null)
    {
        if (topicId <= 0) throw new ArgumentException("TopicId moet groter zijn dan 0.", nameof(topicId));
        if (userId.HasValue && userId.Value <= 0) throw new ArgumentException("UserId moet groter zijn dan 0.", nameof(userId));

        var topic = _topicRepository.ReadTopicById(topicId);
        if (topic == null)
            throw new KeyNotFoundException($"Topic met ID {topicId} niet gevonden.");

        var idea = new Idea
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title,
            Text = text,
            UserId = userId,
            Topic = topic,
            ModerationStatus = ModerationStatus.InReview,
            Image = string.IsNullOrWhiteSpace(imageUri) ? null : new Media { Uri = imageUri }
        };

        _manager.ValidateEntity(idea);
        _ideaRepository.AddIdea(idea);
        await Task.CompletedTask;
    }

    public async Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text, int? userId, string imageUri = null)
    {
        if (topicId <= 0) throw new ArgumentException("TopicId moet groter zijn dan 0.", nameof(topicId));
        if (userId.HasValue && userId.Value <= 0) throw new ArgumentException("UserId moet groter zijn dan 0.", nameof(userId));

        var topic = _topicRepository.ReadTopicById(topicId);
        if (topic == null)
            throw new KeyNotFoundException($"Topic met ID {topicId} niet gevonden.");

        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title.Trim();
        var safeText = text?.Trim() ?? "";

        var subPlatformId = topic.Project?.SubPlatformId;

        var moderation = await _aiModerationService.ModerateIdeaAsync(
            safeTitle,
            safeText,
            subPlatformId
        );

        if (moderation.AiUnavailable)
        {
            var reviewIdea = new Idea
            {
                Title = safeTitle,
                Text = safeText,
                UserId = userId,
                Topic = topic,
                ModerationStatus = ModerationStatus.InReview,
                Image = string.IsNullOrWhiteSpace(imageUri) ? null : new Media { Uri = imageUri }
            };

            _manager.ValidateEntity(reviewIdea);
            _ideaRepository.AddIdea(reviewIdea);

            return moderation;
        }

        if (moderation.IsToxic)
        {
            return moderation;
        }

        var idea = new Idea
        {
            Title = safeTitle,
            Text = safeText,
            UserId = userId,
            Topic = topic,
            ModerationStatus = ModerationStatus.Accepted,
            Image = string.IsNullOrWhiteSpace(imageUri) ? null : new Media { Uri = imageUri }
        };

        _manager.ValidateEntity(idea);
        _ideaRepository.AddIdea(idea);

        return new ToxicityResult
        {
            IsToxic = false,
            AiUnavailable = false,
            SuggestedText = "",
            Explanation = "Idee succesvol opgeslagen."
        };
    }

    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null)
    {
        if (project == null) throw new ArgumentNullException(nameof(project), "Project mag niet null zijn.");
        if (topicId.HasValue && topicId.Value <= 0) throw new ArgumentException("TopicId moet groter zijn dan 0.", nameof(topicId));

        IEnumerable<Idea> ideas;

        if (topicId.HasValue)
        {
            ideas = _ideaRepository.ReadIdeasByTopic(project, topicId.Value);
        }
        else
        {
            ideas = _ideaRepository.ReadIdeasByProject(project);
        }

        return ideas.Where(i => i.ModerationStatus == ModerationStatus.Accepted);
    }

    public IEnumerable<Idea> GetIdeasInReviewBySubPlatform(int subPlatformId)
    {
        if (subPlatformId <= 0) throw new ArgumentException("SubPlatformId moet groter zijn dan 0.", nameof(subPlatformId));
        return _ideaRepository.ReadIdeasInReviewBySubPlatform(subPlatformId);
    }

    public IEnumerable<Idea> GetIdeasBySubPlatform(int subPlatformId, int? projectId = null)
    {
        if (subPlatformId <= 0) throw new ArgumentException("SubPlatformId moet groter zijn dan 0.", nameof(subPlatformId));
        if (projectId.HasValue && projectId.Value <= 0) throw new ArgumentException("ProjectId moet groter zijn dan 0.", nameof(projectId));
        
        return _ideaRepository.ReadIdeasBySubPlatform(subPlatformId, projectId);
    }

    public IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId)
    {
        if (subPlatformId <= 0) throw new ArgumentException("SubPlatformId moet groter zijn dan 0.", nameof(subPlatformId));
        return _reactionRepository.ReadReactionsInReviewBySubPlatform(subPlatformId);
    }

    public void ApproveIdea(int ideaId)
    {
        if (ideaId <= 0) throw new ArgumentException("IdeaId moet groter zijn dan 0.", nameof(ideaId));

        var idea = _ideaRepository.ReadIdeaById(ideaId);
        if (idea == null)
            throw new KeyNotFoundException($"Idee met ID {ideaId} niet gevonden.");

        idea.ModerationStatus = ModerationStatus.Accepted;
        _ideaRepository.UpdateIdea(idea);
    }

    public void RejectIdea(int ideaId)
    {
        if (ideaId <= 0) throw new ArgumentException("IdeaId moet groter zijn dan 0.", nameof(ideaId));

        var idea = _ideaRepository.ReadIdeaById(ideaId);
        if (idea == null)
            throw new KeyNotFoundException($"Idee met ID {ideaId} niet gevonden.");

        _ideaRepository.DeleteIdea(ideaId);
    }

    public async Task<string> ImproveIdeaTextAsync(int ideaId)
    {
        if (ideaId <= 0) throw new ArgumentException("IdeaId moet groter zijn dan 0.", nameof(ideaId));

        var idea = _ideaRepository.ReadIdeaById(ideaId);
        
        if (idea == null)
            throw new KeyNotFoundException($"Idee met ID {ideaId} niet gevonden.");

        var subPlatformId = idea.Topic?.Project?.SubPlatformId;
        var config = _aiModelConfigurationManager.GetActiveConfiguration("IdeaImprovement", subPlatformId);

        var prompt = _aiPromptService.BuildIdeaImprovementPrompt(idea.Title, idea.Text);
        var improvedText = await _aiProvider.GenerateAsync(prompt);

        _aiUsageService.RegisterTextUsage(
            "IdeaImprovement",
            config.ModelName,
            prompt,
            improvedText,
            true,
            null,
            subPlatformId
        );

        if (string.IsNullOrWhiteSpace(improvedText))
        {
            throw new InvalidOperationException("AI gaf geen verbeterde tekst terug.");
        }

        return improvedText.Trim();
    }

    public async Task<string> ImproveIdeaTextAsync(string title, string text, string language = "")
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Idea text is required.", nameof(text));

        var prompt = _aiPromptService.BuildIdeaImprovementPrompt(title ?? string.Empty, text, language);
        var improvedText = await _aiProvider.GenerateAsync(prompt);

        if (string.IsNullOrWhiteSpace(improvedText))
        {
            throw new InvalidOperationException("AI returned an empty response.");
        }

        return improvedText.Trim();
    }

    public async Task<List<string>> GenerateIdeaFollowUpQuestionsAsync(string title, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Idea text is required.", nameof(text));

        var config = _aiModelConfigurationManager.GetActiveConfiguration("IdeaFollowUpQuestions", null);
        var prompt = _aiPromptService.BuildIdeaFollowUpQuestionsPrompt(title ?? string.Empty, text);
        var aiResponse = await _aiProvider.GenerateAsync(prompt);

        _aiUsageService.RegisterTextUsage(
            "IdeaFollowUpQuestions",
            config.ModelName,
            prompt,
            aiResponse,
            true
        );

        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            return new List<string>();
        }

        try
        {
            var questions = JsonSerializer.Deserialize<List<string>>(aiResponse);
            return questions?
                       .Where(q => !string.IsNullOrWhiteSpace(q))
                       .Select(q => q.Trim())
                       .Take(2)
                       .ToList()
                   ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task SubmitIdeaWithoutAiModerationAsync(int topicId, string title, string text, int? userId, string imageUri = null)
    {
        if (topicId <= 0) throw new ArgumentException("TopicId moet groter zijn dan 0.", nameof(topicId));
        if (userId.HasValue && userId.Value <= 0) throw new ArgumentException("UserId moet groter zijn dan 0.", nameof(userId));

        var topic = _topicRepository.ReadTopicById(topicId);
        if (topic == null)
            throw new KeyNotFoundException($"Topic met ID {topicId} niet gevonden.");

        var idea = new Idea
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title.Trim(),
            Text = text?.Trim() ?? "",
            UserId = userId,
            Topic = topic,
            ModerationStatus = ModerationStatus.Accepted,
            Image = string.IsNullOrWhiteSpace(imageUri) ? null : new Media { Uri = imageUri }
        };

        _manager.ValidateEntity(idea);
        _ideaRepository.AddIdea(idea);

        await Task.CompletedTask;
    }

    public async Task<ToxicityResult> ModerateIdeaOnlyAsync(string title, string text)
    {
        return await _aiModerationService.ModerateIdeaAsync(
            title ?? string.Empty,
            text ?? string.Empty
        );
    }
    
    public async Task<string> SummarizeIdeaWithFollowUpAnswersAsync(string title, string text, string followUpAnswers)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Idea text is required.", nameof(text));

        if (string.IsNullOrWhiteSpace(followUpAnswers))
        {
            return text.Trim();
        }

        var config = _aiModelConfigurationManager.GetActiveConfiguration("IdeaFollowUpSummary", null);

        var prompt = _aiPromptService.BuildIdeaFollowUpSummaryPrompt(
            title ?? string.Empty,
            text,
            followUpAnswers
        );

        var summary = await _aiProvider.GenerateAsync(prompt);

        _aiUsageService.RegisterTextUsage(
            "IdeaFollowUpSummary",
            config.ModelName,
            prompt,
            summary,
            true
        );

        if (string.IsNullOrWhiteSpace(summary))
        {
            return text.Trim();
        }

        return summary.Trim();
    }
}