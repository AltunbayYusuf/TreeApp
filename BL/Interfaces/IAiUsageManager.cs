using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.BL.Interfaces;

public interface IAiUsageManager
{
    IEnumerable<AiUsage> GetAllUsages();

}