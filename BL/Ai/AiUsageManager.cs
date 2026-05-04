using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiUsageManager : IAiUsageManager
{
    private readonly IAiUsageRepository _aiUsageRepository;

    public AiUsageManager(IAiUsageRepository aiUsageRepository)
    {
        _aiUsageRepository = aiUsageRepository;
    }

    public IEnumerable<AiUsage> GetAllUsages()
    {
        return _aiUsageRepository.ReadAllUsages();
    }
}