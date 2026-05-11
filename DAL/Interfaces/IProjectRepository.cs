using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;

namespace IntegratieProject.DAL.interfaces;

public interface IProjectRepository
{
    Project ReadProject(int projectId);
    Project ReadProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId);
    IEnumerable<Project> ReadProjectsBySubPlatform(int subPlatformId);
    IEnumerable<Project> ReadAllProjects();
    void ChangeProject(Project project);

    void CreateProject(Project project);
    
    void DeleteProject(Project project);
}