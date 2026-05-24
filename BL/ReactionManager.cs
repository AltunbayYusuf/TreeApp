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
        // 1. ID validatie
        ValidateIds(ideaId, userId);
            
        // 2. Business rule: Een reactie moet ofwel tekst, ofwel een emoji hebben (of beide)
        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Een reactie moet minimaal tekst of een emoji bevatten.");

        var idea = _ideaRepository.ReadIdeaById(ideaId);

        // 3. Specifieke exception gebruiken
        if (idea == null)
            throw new KeyNotFoundException($"Idee met ID {ideaId} niet gevonden.");

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

        var subPlatformId = idea.Topic?.Project?.SubPlatformId;

        var moderation = await _aiModerationService.ModerateReactionAsync(
            safeText,
            subPlatformId
        );

        if (moderation.AiUnavailable)
        {
            var reviewReaction = new Reaction
            {
                Idea = idea,
                Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
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
            Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
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
        ValidateIds(ideaId, userId);
        
        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Een reactie moet minimaal tekst of een emoji bevatten.");

        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
            throw new KeyNotFoundException($"Idee met ID {ideaId} niet gevonden.");

        var reaction = new Reaction
        {
            UserId = userId,
            Idea = idea,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
            Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim(),
            ModerationStatus = ModerationStatus.InReview
        };

        _manager.ValidateEntity(reaction);
        _reactionRepository.AddReaction(reaction);

        await Task.CompletedTask;
    }

    public async Task AddReactionWithoutAiAsync(int ideaId, string emoji, string text, int? userId)
    {
        ValidateIds(ideaId, userId);
        
        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Een reactie moet minimaal tekst of een emoji bevatten.");

        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
            throw new KeyNotFoundException($"Idee met ID {ideaId} niet gevonden.");

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
        ValidateIds(ideaId, userId);
        
        if (string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Emoji mag niet leeg zijn.", nameof(emoji));

        var idea = _ideaRepository.ReadIdeaById(ideaId);

        if (idea == null)
            throw new KeyNotFoundException($"Idee met ID {ideaId} niet gevonden.");

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

        _manager.ValidateEntity(reaction);
        _reactionRepository.AddReaction(reaction);

        await Task.CompletedTask;
        return (true, _reactionRepository.CountAcceptedEmojiReactions(ideaId, emoji));
    }

    public IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId)
    {
        if (subPlatformId <= 0)
            throw new ArgumentException("SubPlatformId moet groter zijn dan 0.", nameof(subPlatformId));

        return _reactionRepository.ReadReactionsInReviewBySubPlatform(subPlatformId);
    }

    public void ApproveReaction(int reactionId)
    {
        if (reactionId <= 0)
            throw new ArgumentException("ReactionId moet groter zijn dan 0.", nameof(reactionId));

        var reaction = _reactionRepository.ReadReactionById(reactionId);

        if (reaction == null)
            throw new KeyNotFoundException($"Reactie met ID {reactionId} niet gevonden.");

        reaction.ModerationStatus = ModerationStatus.Accepted;
        _manager.ValidateEntity(reaction);
        _reactionRepository.UpdateReaction(reaction);
    }

    public void RejectReaction(int reactionId)
    {
        if (reactionId <= 0)
            throw new ArgumentException("ReactionId moet groter zijn dan 0.", nameof(reactionId));

        var reaction = _reactionRepository.ReadReactionById(reactionId);

        if (reaction == null)
            throw new KeyNotFoundException($"Reactie met ID {reactionId} niet gevonden.");

        _reactionRepository.DeleteReaction(reactionId);
    }

    private void ValidateIds(int ideaId, int? userId)
    {
        if (ideaId <= 0)
            throw new ArgumentException("IdeaId moet groter zijn dan 0.", nameof(ideaId));
            
        if (userId.HasValue && userId.Value <= 0)
            throw new ArgumentException("UserId moet groter zijn dan 0.", nameof(userId));
    }
}