using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class ProjectManager : IProjectManager
{
    private readonly ISubplatformRepository _subplatformRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IManager _manager;

    public ProjectManager(ISubplatformRepository subplatformRepository, IProjectRepository projectRepository, IManager manager)
    {
        _subplatformRepository = subplatformRepository;
        _projectRepository = projectRepository;
        _manager = manager;
    }

    public Project GetProject(int projectId)
    {
        if (projectId <= 0)
            throw new ArgumentException("ProjectId moet groter zijn dan 0.", nameof(projectId));

        var project = _projectRepository.ReadProject(projectId);
        
        if (project == null)
            throw new KeyNotFoundException($"Project met ID {projectId} werd niet gevonden.");

        return project;
    }

    public IEnumerable<Project> GetProjectsBySubPlatform(int subPlatformId)
    {
        if (subPlatformId <= 0)
            throw new ArgumentException("SubPlatformId moet groter zijn dan 0.", nameof(subPlatformId));

        return _projectRepository.ReadProjectsBySubPlatform(subPlatformId);
    }

    public Project GetFirstProjectBySubPlatform(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug mag niet leeg zijn.", nameof(slug));

        var subPlatform = _subplatformRepository.ReadSubPlatformBySlug(slug);

        if (subPlatform == null)
        {
            throw new KeyNotFoundException($"Subplatform met slug '{slug}' werd niet gevonden.");
        }

        return _projectRepository.ReadProjectsBySubPlatform(subPlatform.Id)
            .OrderBy(p => p.Id).FirstOrDefault();
    }

    public void UpdateProject(Project project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project), "Project mag niet null zijn.");

        _manager.ValidateEntity(project);
        _projectRepository.ChangeProject(project);
    }

    public void CreateProject(Project project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project), "Project mag niet null zijn.");

        _manager.ValidateEntity(project);
        _projectRepository.CreateProject(project);
    } 
    
    public IEnumerable<Project> GetAllProject()
    {
        return _projectRepository.ReadAllProjects();
    }

    public Project GetProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId)
    {
        if (string.IsNullOrWhiteSpace(subplatformSlug))
            throw new ArgumentException("Subplatform slug mag niet leeg zijn.", nameof(subplatformSlug));
            
        if (projectId <= 0)
            throw new ArgumentException("ProjectId moet groter zijn dan 0.", nameof(projectId));

        var project = _projectRepository.ReadProjectBySubPlatformAndProjectId(subplatformSlug, projectId);
        
        if (project == null)
            throw new KeyNotFoundException($"Project met ID {projectId} binnen subplatform '{subplatformSlug}' niet gevonden.");
            
        return project;
    }
    
    public void DeleteProject(int projectId)
    {
        if (projectId <= 0)
            throw new ArgumentException("ProjectId moet groter zijn dan 0.", nameof(projectId));

        var project = _projectRepository.ReadProject(projectId);

        if (project == null)
            throw new KeyNotFoundException($"Project met ID {projectId} werd niet gevonden en kan dus niet verwijderd worden.");

        _projectRepository.DeleteProject(project);
    }
}