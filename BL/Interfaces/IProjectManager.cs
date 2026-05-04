using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;

namespace IntegratieProject.BL.interfaces;

public interface IProjectManager
{
    Project GetProject(int projectId);
    Project GetFirstProjectBySubPlatform(string slug);
    void UpdateProject(Project project);

    void CreateProject(Project project);
    Project GetProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId);
    IEnumerable<Project> GetProjectsBySubPlatform(int subPlatformId);

    void DeleteProject(int projectId);
}