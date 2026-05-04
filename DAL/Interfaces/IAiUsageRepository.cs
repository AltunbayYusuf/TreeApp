using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.DAL.Interfaces;

public interface IAiUsageRepository
{
    void AddUsage(AiUsage usage);
    IEnumerable<AiUsage> ReadAllUsages();
}