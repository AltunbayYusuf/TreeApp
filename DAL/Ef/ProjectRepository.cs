using IntergratieProject.DAL.interfaces;
using IntergratieProject.Domain.project;
using Microsoft.EntityFrameworkCore;

namespace IntergratieProject.DAL.Ef;

public class ProjectRepository : IProjectRepository
{
    private readonly TreeDbContext _context;

    public ProjectRepository(TreeDbContext context)
    {
        _context = context;
    }

    public Project? ReadProject(int projectId)
    {
        return _context.Projects
            .Include(p => p.SubPlatform)
            .Include(p => p.Photo)
            .Include(p => p.Logo)
            .FirstOrDefault(p => p.Id == projectId);
    }

    public Project? ReadProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId)
    {
        return _context.Projects
            .Include(p => p.SubPlatform)
            .Include(p => p.Photo)
            .Include(p => p.Logo)
            .FirstOrDefault(p => p.Id == projectId && p.SubPlatform.Slug == subplatformSlug);
    }

    public IEnumerable<Project> ReadProjectsBySubPlatform(int subPlatformId)
    {
        return _context.Projects
            .Include(p => p.SubPlatform)
            .Include(p => p.Photo)
            .Include(p => p.Logo)
            .Include(p => p.SurveyResponses)
            .Include(p => p.Topics)
            .ThenInclude(t => t.Ideas)
            .Where(p => p.SubPlatformId == subPlatformId)
            .ToList();
    }

    public void ChangeProject(Project project)
    {
        _context.Projects.Update(project);
        _context.SaveChanges();
    }

    public void CreateProject(Project project)
    {
        _context.Projects.Add(project);
        _context.SaveChanges();
    }
}