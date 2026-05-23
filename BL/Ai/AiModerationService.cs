using System.Text.Json;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiModerationService : IAiModerationService
{
    private readonly IAiProvider _aiProvider;    
    private readonly IAiPromptService _promptService;
    private readonly AiUsageService _aiUsageService;
    private readonly IAiModelConfigurationManager _modelConfigurationManager;


    public AiModerationService(
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

    public async Task<ToxicityResult> ModerateIdeaAsync(string title, string text, int? subPlatformId = null)
    {
        var prompt = _promptService.BuildIdeaModerationPrompt(title, text);
        return await ModerateAsync(prompt, subPlatformId);
    }

    public async Task<ToxicityResult> ModerateReactionAsync(string text, int? subPlatformId = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ToxicityResult
            {
                IsToxic = false,
                AiUnavailable = false,
                SuggestedText = "",
                Explanation = "Geen tekst om te modereren."
            };
        }

        var prompt = _promptService.BuildReactionModerationPrompt(text.Trim());
        return await ModerateAsync(prompt, subPlatformId);
    }

    private async Task<ToxicityResult> ModerateAsync(string prompt, int? subPlatformId)
    {
        var config = _modelConfigurationManager.GetActiveConfiguration("Moderation", subPlatformId);
        try
        {
            var rawText = await _aiProvider.GenerateAsync(prompt);

            var json = ExtractJson(rawText);

            using var doc = JsonDocument.Parse(json);

            var isToxic = doc.RootElement.GetProperty("isToxic").GetBoolean();
            var explanation = doc.RootElement.GetProperty("explanation").GetString() ?? "";
            var suggestedText = doc.RootElement.GetProperty("suggestedText").GetString() ?? "";
            var suggestedTitle = doc.RootElement.TryGetProperty("suggestedTitle", out var suggestedTitleElement)
                ? suggestedTitleElement.GetString() ?? ""
                : "";

            _aiUsageService.RegisterTextUsage(
                "Moderation",
                config.ModelName,
                prompt,
                rawText,
                true,
                null,
                subPlatformId
            );

            return new ToxicityResult
            {
                IsToxic = isToxic,
                AiUnavailable = false,
                SuggestedTitle = suggestedTitle,
                SuggestedText = suggestedText,
                Explanation = explanation
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("AI moderation failed:");
            Console.WriteLine(ex.ToString());

            _aiUsageService.RegisterTextUsage(
                "Moderation",
                config.ModelName,
                prompt,
                "",
                false,
                ex.Message,
                subPlatformId
            );

            return new ToxicityResult
            {
                IsToxic = true,
                AiUnavailable = true,
                SuggestedText = "",
                Explanation = "AI moderation tijdelijk niet beschikbaar. Fout: " + ex.Message
            };
        }
    }

    private static string ExtractJson(string rawText)
    {
        var firstBrace = rawText.IndexOf('{');
        var lastBrace = rawText.LastIndexOf('}');

        if (firstBrace < 0 || lastBrace <= firstBrace)
        {
            throw new InvalidOperationException("Geen geldig JSON-object gevonden in AI response.");
        }

        return rawText.Substring(firstBrace, lastBrace - firstBrace + 1);
    }
}