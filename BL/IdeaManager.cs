using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL;

public class IdeaManager : IIdeaManager
{
    private readonly IIdeaRepository _ideaRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IReactionRepository _reactionRepository;
    private readonly IManager _manager;
    private readonly IAiModerationService _aiModerationService;
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _aiPromptService;
    private readonly IRepository _repository;




    public IdeaManager(IIdeaRepository ideaRepository, ITopicRepository topicRepository,
        IReactionRepository reactionRepository,IManager manager, IAiModerationService aiModerationService, IAiProvider aiProvider, IAiPromptService aiPromptService, IRepository repository)
    {
        _reactionRepository = reactionRepository;
        _topicRepository = topicRepository;
        _ideaRepository = ideaRepository;
        _manager = manager;
        _aiModerationService = aiModerationService;
        _aiProvider = aiProvider;
        _aiPromptService = aiPromptService;
        _repository = repository;
    }
    

    public async Task ForceSubmitIdeaAsync(int topicId, string title, string text, int? userId)
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
            UserId = userId,
            Topic = topic,
            ModerationStatus = ModerationStatus.InReview
        };

        _ideaRepository.AddIdea(idea);
        await Task.CompletedTask;
    }

    public async Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text, int? userId)
    {
        var topic = _repository.ReadTopicById(topicId);
        if (topic == null)
        {
            throw new Exception("Topic niet gevonden");
        }

        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title.Trim();
        var safeText = text?.Trim() ?? "";

        var moderation = await _aiModerationService.ModerateIdeaAsync(safeTitle, safeText);

        if (moderation.AiUnavailable)
        {
            var reviewIdea = new Idea
            {
                Title = safeTitle,
                Text = safeText,
                UserId = userId,
                Topic = topic,
                ModerationStatus = ModerationStatus.InReview
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
            ModerationStatus = ModerationStatus.Accepted
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
        return _ideaRepository.ReadIdeasInReviewBySubPlatform(subPlatformId);
    }

    public IEnumerable<Idea> GetIdeasBySubPlatform(int subPlatformId, int? projectId = null)
    {
        return _ideaRepository.ReadIdeasBySubPlatform(subPlatformId, projectId);
    }

    public IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId)
    {
        return _reactionRepository.ReadReactionsInReviewBySubPlatform(subPlatformId);
    }

    public void ApproveIdea(int ideaId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
        {
            throw new ArgumentException("Idea not found");
        }

        idea.ModerationStatus = ModerationStatus.Accepted;
        _ideaRepository.UpdateIdea(idea);
    }

    public void RejectIdea(int ideaId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
        {
            throw new ArgumentException("Idea not found");
        }

        _ideaRepository.DeleteIdea(ideaId);
    }
    
    public async Task<string> ImproveIdeaTextAsync(int ideaId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
        {
            throw new ArgumentException("Idea not found");
        }

        var prompt = _aiPromptService.BuildIdeaImprovementPrompt(idea.Title, idea.Text);

        var improvedText = await _aiProvider.GenerateAsync(prompt);

        if (string.IsNullOrWhiteSpace(improvedText))
        {
            throw new InvalidOperationException("AI gaf geen verbeterde tekst terug.");
        }

        return improvedText.Trim();
    }
   
    
    public async Task<string> ImproveIdeaTextAsync(string title, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Idea text is required.");
        }

        var prompt = _aiPromptService.BuildIdeaImprovementPrompt(title ?? string.Empty, text);

        var improvedText = await _aiProvider.GenerateAsync(prompt);

        if (string.IsNullOrWhiteSpace(improvedText))
        {
            throw new InvalidOperationException("AI returned an empty response.");
        }

        return improvedText.Trim();
    }
    
}