using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.DAL.interfaces;

public interface ISubplatformRepository
{
    SubPlatform ReadSubPlatformBySlug(string slug);
    SubPlatform ReadSubPlatform(int subPlatformId);
}