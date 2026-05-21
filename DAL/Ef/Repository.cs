using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class Repository : IRepository
{
    private readonly TreeDbContext _context;

    public Repository(TreeDbContext context)
    {
        _context = context;
    }
    public IEnumerable<Topic> ReadTopicsByProject(Project project)
    {
        return _context.Topics.Where(t => t.Project == project).ToList();
    }
    public Topic ReadTopicById(int topicId)
    {
        return _context.Topics
            .Include(t => t.Project)
            .Include(t => t.Ideas)
            .FirstOrDefault(t => t.Id == topicId);
    }
    public SubPlatform ReadSubPlatformBySlug(string slug)
    {
        return _context.SubPlatforms
            .FirstOrDefault(sp => sp.Slug == slug);
    }
    
    public IEnumerable<SubPlatform> ReadAllSubPlatformsWithAdmins()
    {
        return _context.SubPlatforms
            .Include(sp => sp.SubAdmins)
            .ToList();
    }
}