using IntergratieProject.Domain.project;
using IntergratieProject.BL.interfaces;
using IntergratieProject.DAL.interfaces;


namespace IntergratieProject.BL;

public class ProjectManager : IProjectManager
{
    private readonly IRepository _repository;
    private readonly IProjectRepository _projectRepository;
    private readonly IManager _manager;


    public ProjectManager(IRepository repository, IProjectRepository projectRepository)
    {
        _repository = repository;
        _projectRepository = projectRepository;
    }

    public Project? GetProject(int projectId)
    {
        return _projectRepository.ReadProject(projectId);
    }

    public IEnumerable<Project> GetProjectsBySubPlatform(int subPlatformId)
    {
        return _projectRepository.ReadProjectsBySubPlatform(subPlatformId);
    }

    public Project? GetFirstProjectBySubPlatform(string slug)
    {
        var subPlatform = _repository.ReadSubPlatformBySlug(slug);

        if (subPlatform == null)
        {
            return null;
        }

        return _projectRepository.ReadProjectsBySubPlatform(subPlatform.Id)
            .OrderBy(p => p.Id).FirstOrDefault();
    }

    public void UpdateProject(Project project)
    {
        _manager.ValidateEntety(project);
        _projectRepository.ChangeProject(project);
    }

    public void CreateProject(Project project)
    {
        _manager.ValidateEntety(project);
        _projectRepository.CreateProject(project);
    }

    public Project? GetProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId)
    {
        return _projectRepository.ReadProjectBySubPlatformAndProjectId(subplatformSlug, projectId);
    }
}