using IntegratieProject.BL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class ImageGenerationService : IImageGenerationService
{
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _aiPromptService;
    private readonly AiUsageService _aiUsageService;
    private readonly IAiModelConfigurationManager _modelConfigurationManager;

    public ImageGenerationService(
        IAiProvider aiProvider,
        IAiPromptService aiPromptService,
        AiUsageService aiUsageService,
        IAiModelConfigurationManager modelConfigurationManager)
    {
        _aiProvider = aiProvider;
        _aiPromptService = aiPromptService;
        _aiUsageService = aiUsageService;
        _modelConfigurationManager = modelConfigurationManager;
    }

    public async Task<byte[]> GenerateProjectImageAsync(
        string title,
        string description,
        int? subPlatformId = null)
    {
        var config = _modelConfigurationManager.GetActiveConfiguration("ImageGeneration", subPlatformId);

        var prompt =  _aiPromptService.BuildProjectImageGenerationPrompt(
            title,
            description
        );

        try
        {
            var imageBytes = await _aiProvider.GenerateImageAsync(prompt);

            _aiUsageService.RegisterImageUsage(
                "ImageGeneration",
                config.ModelName,
                true,
                null,
                subPlatformId
            );

            return imageBytes;
        }
        catch (Exception ex)
        {
            _aiUsageService.RegisterImageUsage(
                "ImageGeneration",
                config.ModelName,
                false,
                ex.Message,
                subPlatformId
            );

            throw;
        }
    }
}