using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.users;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class SubplatformManager : ISubplatformManager
{
    private readonly ISubplatformRepository _subplatformRepository;


    public SubplatformManager(ISubplatformRepository subplatformRepository)
    {
        _subplatformRepository = subplatformRepository;
    }

    public SubPlatform GetSubPlatformBySlug(string slug)
    {
        return _subplatformRepository.ReadSubPlatformBySlug(slug);
    }

    public SubPlatform GetSubPlatform(int subPlatformId)
    {
        return _subplatformRepository.ReadSubPlatform(subPlatformId);
    }

    public void CreateSubPlatform( string companyName,  string slug,  string contactEmail,  string adminEmail)
    {
        var platform = _subplatformRepository.ReadPlatform();

        var subPlatform = new SubPlatform
        {
            CompanyName = companyName,
            Slug = slug,
            Platform = platform,
            Language = Language.Nl  
        };

        _subplatformRepository.CreateSubPlatform(subPlatform);

        var subAdmin = new SubAdmin
        {
            Name = adminEmail,
            SubPlatform = subPlatform
        };

        _subplatformRepository.CreateSubAdmin(subAdmin);

    }
}