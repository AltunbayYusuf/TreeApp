using System.Text.Json;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiModerationService : IAiModerationService
{
    private readonly IAiProvider _aiProvider;    
    private readonly IAiPromptService _promptService;
    private readonly IAiUsageRepository _aiUsageRepository;

    public AiModerationService(
        IAiProvider aiProvider, 
        IAiPromptService promptService, 
        IAiUsageRepository aiUsageRepository)
    {
        _aiProvider = aiProvider;
        _promptService = promptService;
        _aiUsageRepository = aiUsageRepository;
    }

    public async Task<ToxicityResult> ModerateIdeaAsync(string title, string text)
    {
        var prompt = _promptService.BuildIdeaModerationPrompt(title, text);
        return await ModerateAsync(prompt);
    }

    public async Task<ToxicityResult> ModerateReactionAsync(string text)
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
        return await ModerateAsync(prompt);
    }

    private async Task<ToxicityResult> ModerateAsync(string prompt)
    {
        try
        {
            var rawText = await _aiProvider.GenerateAsync(prompt);

            var json = ExtractJson(rawText);

            using var doc = JsonDocument.Parse(json);

            var isToxic = doc.RootElement.GetProperty("isToxic").GetBoolean();
            var needsMoreDetail = doc.RootElement.GetProperty("needsMoreDetail").GetBoolean();
            var explanation = doc.RootElement.GetProperty("explanation").GetString() ?? "";
            var suggestedText = doc.RootElement.GetProperty("suggestedText").GetString() ?? "";
            var suggestedTitle = doc.RootElement.TryGetProperty("suggestedTitle", out var suggestedTitleElement)
                ? suggestedTitleElement.GetString() ?? ""
                : "";

          
            _aiUsageRepository.AddUsage(new AiUsage
            {
                Feature = "Moderation",
                Model = "gemini-2.5-flash-lite",
                Success = true
            });

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

       
            _aiUsageRepository.AddUsage(new AiUsage
            {
                Feature = "Moderation",
                Model = "gemini-2.5-flash-lite",
                Success = false,
                ErrorMessage = ex.Message
            });

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