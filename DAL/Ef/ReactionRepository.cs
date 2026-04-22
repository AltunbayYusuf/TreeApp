using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.DAL.interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class ReactionRepository : IReactionRepository
{
    private readonly TreeDbContext _context;

    public ReactionRepository(TreeDbContext context)
    {
        _context = context;
    }

    public void AddReaction(Reaction reaction)
    {
        _context.Reactions.Add(reaction);
        _context.SaveChanges();
    }

    public IEnumerable<Reaction> ReadReactionsInReviewBySubPlatform(int subPlatformId)
    {
        return _context.Reactions.Include(r => r.Idea)
            .ThenInclude(i => i.Topic)
            .ThenInclude(t => t.Project)
            .Where(r => r.Idea.Topic.Project.SubPlatformId == subPlatformId &&
                        r.ModerationStatus == ModerationStatus.InReview)
            .ToList();
    }

    public Reaction ReadReactionById(int reactionId)
    {
        return _context.Reactions
            .Include(r => r.Idea)
            .FirstOrDefault(r => r.Id == reactionId);
    }

    public void UpdateReaction(Reaction reaction)
    {
        _context.Reactions.Update(reaction);
        _context.SaveChanges();
    }

    public void DeleteReaction(int reactionId)
    {
        var reaction = _context.Reactions.FirstOrDefault(r => r.Id == reactionId);

        if (reaction == null)
        {
            return;
        }

        _context.Reactions.Remove(reaction);
        _context.SaveChanges();
    }
}