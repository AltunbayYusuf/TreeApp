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
        var project = _projectRepository.ReadProject(projectId);
        return project;
    }

    public IEnumerable<Project> GetProjectsBySubPlatform(int subPlatformId)
    {

        return _projectRepository.ReadProjectsBySubPlatform(subPlatformId);
    }

    public Project GetFirstProjectBySubPlatform(string slug)
    {
        
        var subPlatform = _subplatformRepository.ReadSubPlatformBySlug(slug);

        if (subPlatform == null) return null;

        return _projectRepository.ReadProjectsBySubPlatform(subPlatform.Id)
            .OrderBy(p => p.Id).FirstOrDefault();
    }

    public void UpdateProject(Project project)
    {
       
        _manager.ValidateEntity(project);
        _projectRepository.ChangeProject(project);
    }

    public void CreateProject(Project project)
    {
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
            return null;
        if (projectId <= 0)
            return null;

        var project = _projectRepository.ReadProjectBySubPlatformAndProjectId(subplatformSlug, projectId);

        if (project == null)
            return null;
        return project;
    }
    
    public void DeleteProject(int projectId)
    {
        
        var project = _projectRepository.ReadProject(projectId);
        
        _manager.ValidateEntity(project);
        _projectRepository.DeleteProject(project);
    }
}