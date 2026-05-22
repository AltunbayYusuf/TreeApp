using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class AiModelConfigurationRepository : IAiModelConfigurationRepository
{
    private readonly TreeDbContext _context;

    public AiModelConfigurationRepository(TreeDbContext context)
    {
        _context = context;
    }

    public AiModelConfiguration ReadActiveConfiguration(string featureKey, int? subPlatformId)
    {
        return _context.AiModelConfigurations
            .Include(c => c.SubPlatform)
            .Where(c => c.IsActive && c.FeatureKey == featureKey)
            .Where(c => c.SubPlatformId == subPlatformId || c.SubPlatformId == null)
            .OrderByDescending(c => c.SubPlatformId != null)
            .FirstOrDefault();
    }

    public IEnumerable<AiModelConfiguration> ReadAllConfigurations()
    {
        return _context.AiModelConfigurations
            .Include(c => c.SubPlatform)
            .OrderBy(c => c.FeatureKey)
            .ThenBy(c => c.SubPlatformId)
            .ToList();
    }

    public AiModelConfiguration ReadConfiguration(int id)
    {
        return _context.AiModelConfigurations
            .Include(c => c.SubPlatform)
            .FirstOrDefault(c => c.Id == id);
    }

    public void CreateConfiguration(AiModelConfiguration configuration)
    {
        _context.AiModelConfigurations.Add(configuration);
        _context.SaveChanges();
    }

    public void UpdateConfiguration(AiModelConfiguration configuration)
    {
        _context.AiModelConfigurations.Update(configuration);
        _context.SaveChanges();
    }
}