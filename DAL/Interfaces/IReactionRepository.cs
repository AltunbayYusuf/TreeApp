using IntegratieProject.BL.Domain.ideas;

namespace IntegratieProject.DAL.interfaces;

public interface IReactionRepository
{
    void AddReaction(Reaction reaction);
    IEnumerable<Reaction> ReadReactionsInReviewBySubPlatform(int subPlatformId);
    Reaction ReadReactionById(int reactionId);
    Reaction ReadAcceptedEmojiReaction(int ideaId, int userId, string emoji);
    IEnumerable<Reaction> ReadAcceptedEmojiReactionsForUser(int ideaId, int userId);
    int CountAcceptedEmojiReactions(int ideaId, string emoji);
    void UpdateReaction(Reaction reaction);
    void DeleteReaction(int reactionId);
}