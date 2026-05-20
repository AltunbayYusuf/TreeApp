using System.Text.Json;
using IntegratieProject.BL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiSurveyGenerationService : IAiSurveyGenerationService
{
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _promptService;
    private readonly AiUsageService _aiUsageService;
    private readonly IAiModelConfigurationManager _modelConfigurationManager;

    public AiSurveyGenerationService(
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

    public async Task<SurveyGenerationResult> GenerateSurveyAsync(
        string description,
        int questionAmount,
        int? subPlatformId = null)
    {
        var config = _modelConfigurationManager.GetActiveConfiguration("SurveyGeneration", subPlatformId);
        var prompt = _promptService.BuildSurveyGenerationPrompt(description, questionAmount);

        try
        {
            var rawResponse = await _aiProvider.GenerateAsync(prompt);

            _aiUsageService.RegisterTextUsage(
                "SurveyGeneration",
                config.ModelName,
                prompt,
                rawResponse,
                true,
                null,
                subPlatformId
            );

            var json = ExtractJson(rawResponse);

            var result = JsonSerializer.Deserialize<SurveyGenerationResult>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null)
                throw new InvalidOperationException("AI gaf geen geldige survey terug.");

            return result;
        }
        catch (Exception ex)
        {
            _aiUsageService.RegisterTextUsage(
                "SurveyGeneration",
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

    private static string ExtractJson(string rawResponse)
    {
        var firstBrace = rawResponse.IndexOf('{');
        var lastBrace = rawResponse.LastIndexOf('}');

        if (firstBrace < 0 || lastBrace <= firstBrace)
            throw new InvalidOperationException("AI response bevat geen geldig JSON-object.");

        return rawResponse.Substring(firstBrace, lastBrace - firstBrace + 1);
    }
}