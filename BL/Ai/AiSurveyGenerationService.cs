using System.Text.Json;
using IntegratieProject.BL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiSurveyGenerationService : IAiSurveyGenerationService
{
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _promptService;


    public AiSurveyGenerationService(
        IAiProvider aiProvider,
        IAiPromptService promptService)
    {
        _aiProvider = aiProvider;
        _promptService = promptService;
    }

    public async Task<SurveyGenerationResult> GenerateSurveyAsync(string description, int questionAmount)
    {
        var prompt = _promptService.BuildSurveyGenerationPrompt(description, questionAmount);

        var rawResponse = await _aiProvider.GenerateAsync(prompt);
        
        var json = ExtractJson(rawResponse);
        Console.WriteLine(json);
        var result = JsonSerializer.Deserialize<SurveyGenerationResult>(
            json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null)
            throw new InvalidOperationException("AI gaf geen geldige survey terug.");


        return result;
    }

    private static string ExtractJson(string rawResponse)
    {
        var firstBrace = rawResponse.IndexOf('{');
        var lastBrace = rawResponse.LastIndexOf('}');

        if (firstBrace < 0 || lastBrace <= firstBrace)
        {
            throw new InvalidOperationException("AI response bevat geen geldig JSON-object.");
        }

        return rawResponse.Substring(firstBrace, lastBrace - firstBrace + 1);
    }
}