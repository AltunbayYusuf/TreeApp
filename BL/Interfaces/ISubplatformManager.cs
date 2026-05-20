using IntegratieProject.BL.Domain.project;
using Microsoft.AspNetCore.Http;

namespace IntegratieProject.BL.interfaces;

public interface ISubplatformManager
{
    SubPlatform GetSubPlatformBySlug(string slug);
    SubPlatform GetSubPlatform(int subPlatformId);
    void CreateSubPlatform(SubPlatform subPlatform);
    
    Task<string> CreateSubPlatformAsync(string companyName, string slug, string adminEmail, IFormFile logoFile = null);
    void UpdateSubPlatformLogo(string slug, string logoUri);
    
}