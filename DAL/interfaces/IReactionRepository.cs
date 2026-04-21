using IntergratieProject.Domain.ideas;

namespace IntergratieProject.DAL.interfaces;

public interface IReactionRepository
{
    void AddReaction(Reaction reaction);
    IEnumerable<Reaction> ReadReactionsInReviewBySubPlatform(int subPlatformId);
    Reaction? ReadReactionById(int reactionId);
    void UpdateReaction(Reaction reaction);
    void DeleteReaction(int reactionId);
}