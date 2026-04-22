using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.interfaces;

public interface ISubplatformManager
{
    SubPlatform GetSubPlatformBySlug(string slug);
}