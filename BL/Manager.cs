using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class Manager : IManager
{
    private readonly IAiService _aiService;
    private readonly IRepository _repository;

    public Manager(IAiService aiService, IRepository repository)
    {
        _aiService = aiService;
        _repository = repository;
    }

    public async Task<ToxicityResult> ModerateTextAsync(string input)
    {
        var prompt =
            """
            Je bent een moderatie-assistent voor een jongerenplatform.
            Antwoord ALLEEN met een JSON object. Geen markdown, geen code fences, geen extra tekst.

            JSON schema:
            {"isToxic":true/false,"explanation":"...","suggestedText":"..."}

            Regels:
            - isToxic=true bij schelden, haatspraak, bedreiging, intimidatie, vernedering
            - suggestedText: respectvolle herformulering met dezelfde bedoeling (leeg als niet toxisch)

            INPUT:
            """ + input;

        var aiText = await _aiService.GenerateAsync(prompt, FeatureType.Moderation);

        //debugging
        Console.WriteLine("RAW AI TEXT:");
        Console.WriteLine(aiText);
        Console.WriteLine("------");

        // strip ```json fences als die er zijn
        var cleaned = aiText.Trim();
        var firstBrace = cleaned.IndexOf('{');
        var lastBrace = cleaned.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        try
        {
            using var doc = JsonDocument.Parse(cleaned);

            bool isToxic = doc.RootElement.GetProperty("isToxic").GetBoolean();
            string explanation = doc.RootElement.GetProperty("explanation").GetString() ?? "";
            string suggestedText = doc.RootElement.GetProperty("suggestedText").GetString() ?? "";

            return new ToxicityResult
            {
                IsToxic = isToxic,
                AiUnavailable = false,
                SuggestedText = suggestedText,
                Explanation = explanation
            };
        }
        // catch (Exception ex)
        // {
        //     // AI faalde / output niet parsebaar
        //     // return new ToxicityResult
        //     // {
        //     //     IsToxic = true,
        //     //     AiUnavailable = true,
        //     //     SuggestedText = "",
        //     //     Explanation = $"Moderation check failed: {ex.Message}. Raw: {aiText}"
        //     // };
        //         throw new Exception("AI moderation tijdelijk niet beschikbaar.", ex);
        //
        // }
        catch (Exception ex)
        {
            throw new Exception($"AI moderation tijdelijk niet beschikbaar. Raw AI response: {aiText}", ex);
        }
    }

    public void ValidateEntity(Object model)
    {
        var validationResults = new List<ValidationResult>();
        bool success = Validator.TryValidateObject(model,
            new ValidationContext(model), validationResults, true);

        if (!success)
        {
            var message = "";
            foreach (var validationResult in validationResults)
            {
                message += validationResult.ErrorMessage + " ";
            }

            throw new ValidationException(message);
        }
    }
}