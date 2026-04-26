using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class IdeaManager : IIdeaManager
{
    private readonly IAiService _aiService;
    private readonly IIdeaRepository _ideaRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IReactionRepository _reactionRepository;
    private readonly IManager _manager;


    public IdeaManager(IAiService aiService, IIdeaRepository ideaRepository, ITopicRepository topicRepository,
        IReactionRepository reactionRepository,IManager manager)
    {
        _aiService = aiService;
        _reactionRepository = reactionRepository;
        _topicRepository = topicRepository;
        _ideaRepository = ideaRepository;
        _manager = manager;
    }

    public void SubmitIdeaFromChatAsync(int topicId, string text)
    {
    }

    public async Task<string> AskAiForIdea(string idea)
    {
        var prompt = $"Analyseer dit idee en geef feedback:\n{idea}";

        return await _aiService.GenerateAsync(prompt, FeatureType.IdeaSelection);
    }

    public async Task ForceSubmitIdeaAsync(int topicId, string title, string text, int? userId)
    {
        var topic = _topicRepository.ReadTopicById(topicId);
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
        var topic = _topicRepository.ReadTopicById(topicId);
        if (topic == null)
        {
            throw new Exception("Topic niet gevonden");
        }

        var moderation = await _manager.ModerateTextAsync(text);

        if (moderation.IsToxic)
        {
            return moderation;
        }

        var idea = new Idea
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title,
            Text = text,
            UserId = userId,
            Topic = topic,
            ModerationStatus = ModerationStatus.Accepted
        };
        _manager.ValidateEntity(idea);

        _ideaRepository.AddIdea(idea);

        return new ToxicityResult
        {
            IsToxic = false,
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
}