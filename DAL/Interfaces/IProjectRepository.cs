using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.DAL.interfaces;

public interface IProjectRepository
{
    Project ReadProject(int projectId);
    Project ReadProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId);
    IEnumerable<Project> ReadProjectsBySubPlatform(int subPlatformId);
    void ChangeProject(Project project);

    void CreateProject(Project project);
}