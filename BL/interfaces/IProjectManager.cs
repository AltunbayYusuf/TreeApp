using IntergratieProject.Domain.project;

namespace IntergratieProject.BL.interfaces;

public interface IProjectManager
{
    Project? GetProject(int projectId);
    Project? GetFirstProjectBySubPlatform(string slug);
    void UpdateProject(Project project);

    void CreateProject(Project project);
    Project? GetProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId);
    IEnumerable<Project> GetProjectsBySubPlatform(int subPlatformId);


}