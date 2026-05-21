using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.DAL.Interfaces;

public interface IAiModelConfigurationRepository
{
    AiModelConfiguration ReadActiveConfiguration(string featureKey, int? subPlatformId);
    IEnumerable<AiModelConfiguration> ReadAllConfigurations();
    AiModelConfiguration ReadConfiguration(int id);
    void CreateConfiguration(AiModelConfiguration configuration);
    void UpdateConfiguration(AiModelConfiguration configuration);
}