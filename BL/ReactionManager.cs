using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class ReactionManager : IReactionManager
{
    private readonly IReactionRepository _repository;
    private readonly IIdeaRepository _ideaRepository;
    private readonly IManager _manager;


    public ReactionManager(IReactionRepository repository, IIdeaRepository ideaRepository,IManager manager)
    {
        _repository = repository;
        _ideaRepository = ideaRepository;
        _manager = manager;
    }


    public async Task ForceAddReactionAsync(int ideaId, string emoji, string text, int? userId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

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

    public async Task<ToxicityResult> AddReaction(int ideaId, string emoji, string text, int? userId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

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
        var moderation = await _manager.ModerateTextAsync(text);

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

    public async Task<(bool Added, int Count)> ToggleEmojiReactionAsync(int ideaId, string emoji, int userId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
        {
            throw new Exception("Idee niet gevonden");
        }

        var existingReaction = _repository.ReadAcceptedEmojiReaction(ideaId, userId, emoji);

        if (existingReaction != null)
        {
            _repository.DeleteReaction(existingReaction.Id);
            return (false, _repository.CountAcceptedEmojiReactions(ideaId, emoji));
        }

        var otherEmojiReactions = _repository.ReadAcceptedEmojiReactionsForUser(ideaId, userId)
            .Where(r => r.Emoji != emoji)
            .ToList();

        foreach (var otherReaction in otherEmojiReactions)
        {
            _repository.DeleteReaction(otherReaction.Id);
        }

        var reaction = new Reaction
        {
            UserId = userId,
            Idea = idea,
            Emoji = emoji,
            Text = null,
            ModerationStatus = ModerationStatus.Accepted
        };

        _repository.AddReaction(reaction);
        return (true, _repository.CountAcceptedEmojiReactions(ideaId, emoji));
    }

    public IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId)
    {
        return _repository.ReadReactionsInReviewBySubPlatform(subPlatformId);
    }

    public void ApproveReaction(int reactionId)
    {
        var reaction = _repository.ReadReactionById(reactionId);

        if (reaction == null)
        {
            throw new ArgumentException("Reaction not found");
        }

        reaction.ModerationStatus = ModerationStatus.Accepted;
        _repository.UpdateReaction(reaction);
    }

    public void RejectReaction(int reactionId)
    {
        var reaction = _repository.ReadReactionById(reactionId);

        if (reaction == null)
        {
            throw new ArgumentException("Reaction not found");
        }

        _repository.DeleteReaction(reactionId);
    }
}