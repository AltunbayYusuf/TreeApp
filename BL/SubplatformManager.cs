using IntegratieProject.BL.Domain.project;
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
}