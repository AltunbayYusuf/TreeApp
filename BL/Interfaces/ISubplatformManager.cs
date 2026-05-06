using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.interfaces;

public interface ISubplatformManager
{
    SubPlatform GetSubPlatformBySlug(string slug);
    SubPlatform GetSubPlatform(int subPlatformId);
    void CreateSubPlatform(SubPlatform subPlatform);
    
    Task<string> CreateSubPlatformAsync(string companyName, string slug, string adminEmail);
}