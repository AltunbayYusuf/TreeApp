using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.users;

namespace IntegratieProject.DAL.interfaces;

public interface ISubplatformRepository
{
    SubPlatform ReadSubPlatformBySlug(string slug);
    SubPlatform ReadSubPlatform(int subPlatformId);
    Platform ReadPlatform();

    void CreateSubPlatform(SubPlatform subPlatform);
    void CreateSubAdmin(SubAdmin subAdmin);
    
    void UpdateSubPlatform(SubPlatform subPlatform);
}