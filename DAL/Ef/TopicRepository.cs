using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.DAL.interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class TopicRepository : ITopicRepository
{
    private readonly TreeDbContext _context;

    public TopicRepository(TreeDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Topic> ReadTopicsByProject(Project project)
    {
        return _context.Topics.Where(t => t.Project == project).ToList();
    }

    public IEnumerable<Topic> ReadTopicsBySubPlatform(int subPlatformId)
    {
        return _context.Topics
            .AsNoTracking()
            .Where(t => t.Project != null && t.Project.SubPlatformId == subPlatformId)
            .OrderBy(t => t.Theme)
            .ToList();
    }

    public Topic ReadTopicById(int topicId)
    {
        return _context.Topics
            .Include(t => t.Project)
            .Include(t => t.Ideas)
            .FirstOrDefault(t => t.Id == topicId);
    }
}