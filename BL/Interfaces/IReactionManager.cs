using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;

namespace IntegratieProject.BL.interfaces;

public interface IReactionManager
{
    Task<ToxicityResult> AddReaction(int ideaId, string emoji, string text, int? userId);
    Task ForceAddReactionAsync(int ideaId, string emoji, string text, int? userId);
    IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId);
    void ApproveReaction(int reactionId);
    void RejectReaction(int reactionId);
}