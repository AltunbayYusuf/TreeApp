using IntegratieProject.BL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class IntroTextService : IIntroTextService
{
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _promptService;
    private readonly AiUsageService _aiUsageService;
    private readonly IAiModelConfigurationManager _modelConfigurationManager;

    public IntroTextService(
        IAiProvider aiProvider,
        IAiPromptService promptService,
        AiUsageService aiUsageService,
        IAiModelConfigurationManager modelConfigurationManager)
    {
        _aiProvider = aiProvider;
        _promptService = promptService;
        _aiUsageService = aiUsageService;
        _modelConfigurationManager = modelConfigurationManager;
    }

    public async Task<string> GenerateIntroAsync(string projectName, int? subPlatformId = null)
    {
        var config = _modelConfigurationManager.GetActiveConfiguration("IntroGeneration", subPlatformId);
        var prompt = _promptService.BuildProjectIntroPrompt(projectName);

        try
        {
            var result = await _aiProvider.GenerateAsync(prompt);

            _aiUsageService.RegisterTextUsage(
                "IntroGeneration",
                config.ModelName,
                prompt,
                result,
                true,
                null,
                subPlatformId
            );

            return result;
        }
        catch (Exception ex)
        {
            _aiUsageService.RegisterTextUsage(
                "IntroGeneration",
                config.ModelName,
                prompt,
                "",
                false,
                ex.Message,
                subPlatformId
            );

            throw;
        }
    }
}