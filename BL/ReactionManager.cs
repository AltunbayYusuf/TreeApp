using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class ReactionManager : IReactionManager
{
    private readonly IAiModerationService _aiModerationService;
    private readonly IReactionRepository _reactionRepository;
    private readonly IIdeaRepository _ideaRepository;
    private readonly IManager _manager;

    public ReactionManager(
        IReactionRepository reactionRepository,
        IIdeaRepository ideaRepository,
        IManager manager,
        IAiModerationService aiModerationService)
    {
        _reactionRepository = reactionRepository;
        _ideaRepository = ideaRepository;
        _manager = manager;
        _aiModerationService = aiModerationService;
    }

    public async Task<ToxicityResult> AddReaction(int ideaId, string emoji, string text, int? userId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
            throw new Exception("Idee niet gevonden");

        var safeText = text?.Trim();

        if (string.IsNullOrWhiteSpace(safeText))
        {
            var emojiReaction = new Reaction
            {
                Idea = idea,
                Emoji = emoji,
                Text = null,
                UserId = userId,
                ModerationStatus = ModerationStatus.Accepted
            };

            _manager.ValidateEntity(emojiReaction);
            _reactionRepository.AddReaction(emojiReaction);

            return new ToxicityResult
            {
                IsToxic = false,
                AiUnavailable = false,
                SuggestedText = "",
                Explanation = "Reactie toegevoegd."
            };
        }

        var moderation = await _aiModerationService.ModerateReactionAsync(safeText);

        if (moderation.AiUnavailable)
        {
            var reviewReaction = new Reaction
            {
                Idea = idea,
                Emoji = emoji,
                Text = safeText,
                UserId = userId,
                ModerationStatus = ModerationStatus.InReview
            };

            _manager.ValidateEntity(reviewReaction);
            _reactionRepository.AddReaction(reviewReaction);

            return moderation;
        }

        if (moderation.IsToxic)
            return moderation;

        var acceptedReaction = new Reaction
        {
            Idea = idea,
            Emoji = emoji,
            Text = safeText,
            UserId = userId,
            ModerationStatus = ModerationStatus.Accepted
        };

        _manager.ValidateEntity(acceptedReaction);
        _reactionRepository.AddReaction(acceptedReaction);

        return new ToxicityResult
        {
            IsToxic = false,
            AiUnavailable = false,
            SuggestedText = "",
            Explanation = "Reactie toegevoegd."
        };
    }

    public async Task ForceAddReactionAsync(int ideaId, string emoji, string text, int? userId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
            throw new Exception("Idee niet gevonden");

        var reaction = new Reaction
        {
            UserId = userId,
            Idea = idea,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
            Text = string.IsNullOrWhiteSpace(text) ? null : text,
            ModerationStatus = ModerationStatus.InReview
        };

        _manager.ValidateEntity(reaction);
        _reactionRepository.AddReaction(reaction);

        await Task.CompletedTask;
    }
    public async Task AddReactionWithoutAiAsync(int ideaId, string emoji, string text, int? userId)
    {
        Idea idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
        {
            throw new Exception("Idee niet gevonden");
        }

        var reaction = new Reaction
        {
            Idea = idea,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
            Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim(),
            UserId = userId,
            ModerationStatus = ModerationStatus.Accepted
        };

        _manager.ValidateEntity(reaction);
        _reactionRepository.AddReaction(reaction);

        await Task.CompletedTask;
    }

    public async Task<(bool Added, int Count)> ToggleEmojiReactionAsync(int ideaId, string emoji, int userId)
    {
        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
            throw new Exception("Idee niet gevonden");

        var existingReaction = _reactionRepository.ReadAcceptedEmojiReaction(ideaId, userId, emoji);

        if (existingReaction != null)
        {
            _reactionRepository.DeleteReaction(existingReaction.Id);
            return (false, _reactionRepository.CountAcceptedEmojiReactions(ideaId, emoji));
        }

        var otherEmojiReactions = _reactionRepository
            .ReadAcceptedEmojiReactionsForUser(ideaId, userId)
            .Where(r => r.Emoji != emoji)
            .ToList();

        foreach (var otherReaction in otherEmojiReactions)
        {
            _reactionRepository.DeleteReaction(otherReaction.Id);
        }

        var reaction = new Reaction
        {
            UserId = userId,
            Idea = idea,
            Emoji = emoji,
            Text = null,
            ModerationStatus = ModerationStatus.Accepted
        };

        _reactionRepository.AddReaction(reaction);

        await Task.CompletedTask;
        return (true, _reactionRepository.CountAcceptedEmojiReactions(ideaId, emoji));
    }

    public IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId)
    {
        return _reactionRepository.ReadReactionsInReviewBySubPlatform(subPlatformId);
    }

    public void ApproveReaction(int reactionId)
    {
        var reaction = _reactionRepository.ReadReactionById(reactionId);

        if (reaction == null)
            throw new ArgumentException("Reaction not found");

        reaction.ModerationStatus = ModerationStatus.Accepted;
        _reactionRepository.UpdateReaction(reaction);
    }

    public void RejectReaction(int reactionId)
    {
        var reaction = _reactionRepository.ReadReactionById(reactionId);

        if (reaction == null)
            throw new ArgumentException("Reaction not found");

        _reactionRepository.DeleteReaction(reactionId);
    }
}