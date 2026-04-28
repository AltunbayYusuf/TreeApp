using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.DAL.Ef;

public class AiUsageRepository : IAiUsageRepository
{
    private readonly TreeDbContext _context;

    public AiUsageRepository(TreeDbContext context)
    {
        _context = context;
    }

    public void AddUsage(AiUsage usage)
    {
        _context.AiUsages.Add(usage);
        _context.SaveChanges();
    }

    public IEnumerable<AiUsage> ReadAllUsages()
    {
        return _context.AiUsages
            .OrderByDescending(u => u.CreatedAt)
            .ToList();
    }
}