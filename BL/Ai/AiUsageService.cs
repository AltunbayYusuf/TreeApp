using IntegratieProject.BL.Interfaces;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiUsageService
{
    private readonly IAiUsageRepository _aiUsageRepository;
    private readonly IAiModelConfigurationManager _modelConfigurationManager;

    public AiUsageService(
        IAiUsageRepository aiUsageRepository,
        IAiModelConfigurationManager modelConfigurationManager)
    {
        _aiUsageRepository = aiUsageRepository;
        _modelConfigurationManager = modelConfigurationManager;
    }

    public void RegisterTextUsage(
        string feature,
        string model,
        string inputText,
        string outputText,
        bool success,
        string errorMessage = null,
        int? subPlatformId = null)
    {
        var config = _modelConfigurationManager.GetActiveConfiguration(feature, subPlatformId);

        var inputTokens = EstimateTokens(inputText);
        var outputTokens = EstimateTokens(outputText);

        var cost =
            (inputTokens / 1_000_000m * config.InputCostPerMillionTokens) +
            (outputTokens / 1_000_000m * config.OutputCostPerMillionTokens);

        _aiUsageRepository.AddUsage(new AiUsage
        {
            Feature = feature,
            Model = model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            TotalTokens = inputTokens + outputTokens,
            EstimatedCost = cost,
            Currency = config.Currency,
            Success = success,
            ErrorMessage = errorMessage ?? "",
            SubPlatformId = subPlatformId
        });
    }

    public void RegisterImageUsage(
        string feature,
        string model,
        bool success,
        string errorMessage = null,
        int? subPlatformId = null)
    {
        var config = _modelConfigurationManager.GetActiveConfiguration(feature, subPlatformId);

        _aiUsageRepository.AddUsage(new AiUsage
        {
            Feature = feature,
            Model = model,
            InputTokens = 0,
            OutputTokens = 0,
            TotalTokens = 0,
            EstimatedCost = config.ImageCostPerImage,
            Currency = config.Currency,
            Success = success,
            ErrorMessage = errorMessage ?? "",
            SubPlatformId = subPlatformId
        });
    }

    private static int EstimateTokens(string text)
    {
        return text.Length / 4;
    }
}