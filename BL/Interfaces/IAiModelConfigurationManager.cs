using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.BL.Interfaces;

public interface IAiModelConfigurationManager
{
    AiModelConfiguration GetActiveConfiguration(string featureKey, int? subPlatformId);
    IEnumerable<AiModelConfiguration> GetAllConfigurations();
    AiModelConfiguration GetConfiguration(int id);
    void AddConfiguration(AiModelConfiguration configuration);
    void ChangeConfiguration(AiModelConfiguration configuration);
}