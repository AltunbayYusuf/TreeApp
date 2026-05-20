using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiModelConfigurationManager : IAiModelConfigurationManager
{
    private readonly IAiModelConfigurationRepository _repository;

    public AiModelConfigurationManager(IAiModelConfigurationRepository repository)
    {
        _repository = repository;
    }

    public AiModelConfiguration GetActiveConfiguration(string featureKey, int? subPlatformId)
    {
        var configuration = _repository.ReadActiveConfiguration(featureKey, subPlatformId);

        if (configuration == null)
        {
            throw new InvalidOperationException(
                $"Geen actieve AI-modelconfiguratie gevonden voor feature '{featureKey}'.");
        }

        return configuration;
    }

    public IEnumerable<AiModelConfiguration> GetAllConfigurations()
    {
        return _repository.ReadAllConfigurations();
    }

    public AiModelConfiguration GetConfiguration(int id)
    {
        return _repository.ReadConfiguration(id);
    }

    public void AddConfiguration(AiModelConfiguration configuration)
    {
        _repository.CreateConfiguration(configuration);
    }

    public void ChangeConfiguration(AiModelConfiguration configuration)
    {
        _repository.UpdateConfiguration(configuration);
    }
}