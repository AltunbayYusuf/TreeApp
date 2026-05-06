using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.interfaces;

public interface IManager
{
    public void ValidateEntity(Object model);
    IEnumerable<SubPlatform> GetAllSubPlatformsWithAdmins();
}