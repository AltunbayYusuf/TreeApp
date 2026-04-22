using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;

namespace IntergratieProject.BL.interfaces;

public interface IReactionManager
{
    Task<ToxicityResult> AddReaction(int ideaId, string emoji, string text, int? userId);
    Task ForceAddReactionAsync(int ideaId, string? emoji, string? text, int? userId);
    IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId);
    void ApproveReaction(int reactionId);
    void RejectReaction(int reactionId);
}