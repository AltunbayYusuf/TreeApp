using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.DAL.interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class IdeaRepository : IIdeaRepository
{
    private readonly TreeDbContext _context;

    public IdeaRepository(TreeDbContext context)
    {
        _context = context;
    }

    public void AddIdea(Idea idea)
    {
        
        _context.Ideas.Add(idea);
        _context.SaveChanges();
    }

    public IEnumerable<Idea> ReadIdeasByProject(Project project)
    {
        return _context.Ideas.Include(i => i.Topic)
            // .Include(i => i.Reactions)
            .Include(i => i.Reactions.Where(r => r.ModerationStatus == ModerationStatus.Accepted))
            .Where(i => i.Topic.Project == project)
            .ToList();
    }


    public IEnumerable<Idea> ReadIdeasByTopic(Project project, int topicId)
    {
        return _context.Ideas.Include(i => i.Topic)
            //  .Include(i => i.Reactions)
            .Include(i => i.Reactions.Where(r => r.ModerationStatus == ModerationStatus.Accepted))
            .Where(i => i.Topic.Project == project && i.Topic.Id == topicId)
            .ToList();
    }


    public Idea ReadIdeaById(int ideaId)
    {
        return _context.Ideas.Include(i => i.Topic)
            .Include(i => i.Reactions)
            .FirstOrDefault(i => i.Id == ideaId);
    }


    public IEnumerable<Idea> ReadIdeasInReviewBySubPlatform(int subPlatformId)
    {
        return _context.Ideas.Include(i => i.Topic)
            .ThenInclude(t => t.Project)
            .Where(i => i.Topic.Project.SubPlatformId == subPlatformId &&
                        i.ModerationStatus == ModerationStatus.InReview)
            .ToList();
    }


    public IEnumerable<Idea> ReadIdeasBySubPlatform(int subPlatformId, int? projectId = null)
    {
        var query = _context.Ideas
            .Include(i => i.Topic)
            .ThenInclude(t => t.Project)
            .Include(i => i.Reactions)
            .Where(i => i.Topic.Project.SubPlatformId == subPlatformId);

        if (projectId.HasValue)
            query = query.Where(i => i.Topic.Project.Id == projectId.Value);

        return query.OrderByDescending(i => i.Id).ToList();
    }

    public void UpdateIdea(Idea idea)
    {
        _context.Ideas.Update(idea);
        _context.SaveChanges();
    }


    public void DeleteIdea(int ideaId)
    {
        var idea = _context.Ideas.Include(i => i.Reactions)
            .FirstOrDefault(i => i.Id == ideaId);

        if (idea == null)
        {
            return;
        }

        if (idea.Reactions.Any())
        {
            _context.Reactions.RemoveRange(idea.Reactions);
        }

        _context.Ideas.Remove(idea);
        _context.SaveChanges();
    }
}