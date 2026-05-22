using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiPromptService : IAiPromptService
{
    private readonly IAiRepository _aiRepository;

    public AiPromptService(IAiRepository aiRepository)
    {
        _aiRepository = aiRepository;
    }

    public string BuildIdeaModerationPrompt(string title, string text)
    {
        var prompt = GetPrompt("idea_moderation");

        var promptText = RemoveDutchLanguageInstruction(prompt.PromptText);

        return $"""
                {promptText}

                INPUT:
                TITLE:
                {title}

                TEXT:
                {text}

                OVERRIDE:
                Detecteer de taal van TITLE en TEXT.
                Geef suggestedTitle en suggestedText in exact dezelfde taal terug.
                Vertaal nooit naar Nederlands tenzij TITLE en TEXT Nederlands zijn.
                """;
    }

    public string BuildReactionModerationPrompt(string text)
    {
        var prompt = GetPrompt("reaction_moderation");

        return $"""
                {prompt.PromptText}

                INPUT:
                {text}
                """;
    }
    

    public string BuildProjectIntroPrompt(string projectName)
    {
        var prompt = GetPrompt("project_intro_generation");

        return prompt.PromptText.Replace("{projectName}", projectName);
    }

    public string BuildSurveyGenerationPrompt(string description, int questionAmount)
    {
        var prompt = GetPrompt("survey_generation");
        return prompt.PromptText
            .Replace("{{description}}", description)
            .Replace("{{questionAmount}}", questionAmount.ToString());
    }

    public string BuildIdeaImprovementPrompt(string title, string text, string language = "")
    {
        var prompt = GetPrompt("idea_improvement");

        var promptText = RemoveDutchLanguageInstruction(prompt.PromptText);

        return $"""
                {promptText
                    .Replace("{title}", title)
                    .Replace("{text}", text)}

                OVERRIDE:
                Detecteer de taal van de originele titel en originele inhoud.
                Geef "title" en "text" in exact dezelfde taal terug.
                Vertaal nooit naar Nederlands tenzij de originele titel en inhoud Nederlands zijn.
                {BuildOutputLanguageInstruction(language)}
                """;
    }

    private static string BuildOutputLanguageInstruction(string language)
    {
        return language?.Trim().ToLowerInvariant() switch
        {
            "en" => "De interface staat op Engels. Geef de JSON waarden verplicht in het Engels terug.",
            "nl" => "De interface staat op Nederlands. Geef de JSON waarden in het Nederlands terug.",
            _ => string.Empty
        };
    }

    private static string RemoveDutchLanguageInstruction(string promptText)
    {
        return promptText
            .Replace("- Schrijf in het Nederlands.", "- Behoud de taal van de gebruiker.")
            .Replace("- Antwoord in het Nederlands", "- Behoud de taal van de gebruiker");
    }

    public string BuildProjectTrendSummaryPrompt(string projectData)
    {
        var prompt = GetPrompt("project_trend_summary");

        return prompt.PromptText.Replace("{{projectData}}", projectData);
    }

    public string BuildOpenQuestionSummaryPrompt(string question, string answers)
    {
        var prompt = GetPrompt("open_question_summary");

        return prompt.PromptText
            .Replace("{{question}}", question)
            .Replace("{{answers}}", answers);
    }


    public string BuildIdeaFollowUpQuestionsPrompt(string title, string text)
    {
        var prompt = GetPrompt("idea_follow_up_questions");

        return prompt.PromptText
            .Replace("{title}", title)
            .Replace("{text}", text);
    }

    public string BuildProjectImageGenerationPrompt(string title, string description)
    {
        var prompt = GetPrompt("project_image_generation");

        return prompt.PromptText
            .Replace("{projectName}", title)
            .Replace("{introduction}", description);
    }

    public string BuildIdeaSelectionPrompt(string selectionMode, string projectData)
    {
        var prompt = GetPrompt("idea_selection");

        return prompt.PromptText
            .Replace("{{selectionMode}}", selectionMode)
            .Replace("{{projectData}}", projectData);
    }
    
    public string BuildIdeaFollowUpSummaryPrompt(string title, string text, string followUpAnswers)
    {
        var prompt = GetPrompt("idea_follow_up_summary");

        return prompt.PromptText
            .Replace("{title}", title)
            .Replace("{text}", text)
            .Replace("{followUpAnswers}", followUpAnswers);
    }
    
    private AiPrompt GetPrompt(string key)
    {
        return _aiRepository.ReadAiPromptByKey(key)
               ?? throw new InvalidOperationException($"Prompt '{key}' niet gevonden.");
    }
}