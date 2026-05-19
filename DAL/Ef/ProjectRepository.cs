using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.DAL.interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class ProjectRepository : IProjectRepository
{
    private readonly TreeDbContext _context;

    public ProjectRepository(TreeDbContext context)
    {
        _context = context;
    }

    public Project ReadProject(int projectId)
    {
        return _context.Projects
            .Include(p => p.QuestionList)
            .ThenInclude(ql => ql.Sections)
            .ThenInclude(s => s.Questions)
            .ThenInclude(q => q.Options)

            .Include(p => p.QuestionList)
            .ThenInclude(ql => ql.Sections)
            .ThenInclude(s => s.Questions)
            .ThenInclude(q => q.ConditionalQuestions)
            .ThenInclude(cq => cq.FollowUpQuestion)

            .Include(p => p.SubPlatform)
            .ThenInclude(sp => sp.Logo)
            .Include(p => p.Photo)
            .Include(p => p.Topics)
            .ThenInclude(t => t.Ideas)
            .Include(p => p.SurveyResponses)
            .FirstOrDefault(p => p.Id == projectId);
    }

    public IEnumerable<Project> ReadAllProjects()
    {
        return _context.Projects
            .Include(p => p.QuestionList)
            .ThenInclude(ql => ql.Sections)
            .ThenInclude(s => s.Questions)
            .ThenInclude(q => q.Options)

            .Include(p => p.QuestionList)
            .ThenInclude(ql => ql.Sections)
            .ThenInclude(s => s.Questions)
            .ThenInclude(q => q.ConditionalQuestions)
            .ThenInclude(cq => cq.FollowUpQuestion)

            .Include(p => p.SubPlatform)
            .ThenInclude(sp => sp.Logo)
            .Include(p => p.Photo)
            .Include(p => p.Topics)
            .ThenInclude(t => t.Ideas)
            .Include(p => p.SurveyResponses);
    }

    public Project ReadProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId)
    {
        return _context.Projects
            .Include(p => p.SubPlatform)
            .ThenInclude(sp => sp.Logo)
            .Include(p => p.Photo)
            .FirstOrDefault(p => p.Id == projectId && p.SubPlatform.Slug == subplatformSlug);
    }

    public IEnumerable<Project> ReadProjectsBySubPlatform(int subPlatformId)
    {
        return _context.Projects
            .Include(p => p.SubPlatform)
            .ThenInclude(sp => sp.Logo)
            .Include(p => p.Photo)
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
    
    public void DeleteProject(Project project)
    { 
        if (project == null)
            return;
        
        var deletedProject = ReadProject(project.Id);
        
        if (deletedProject == null)
            return;
        
        _context.Projects.Remove(deletedProject);
        _context.SaveChanges();
    }


}