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
        var prompt = _aiRepository.ReadAiPromptByKey("idea_moderation")
                     ?? throw new InvalidOperationException("Prompt 'idea_moderation' niet gevonden.");

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
        var prompt = _aiRepository.ReadAiPromptByKey("reaction_moderation")
                     ?? throw new InvalidOperationException("Prompt 'reaction_moderation' niet gevonden.");

        return $"""
                {prompt.PromptText}

                INPUT:
                {text}
                """;
    }

    public string BuildProjectImagePrompt(string projectName)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("project_image_generation")
                     ?? throw new InvalidOperationException("Prompt 'project_image_generation' niet gevonden.");

        return prompt.PromptText.Replace("{projectName}", projectName);
    }

    public string BuildProjectIntroPrompt(string projectName)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("project_intro_generation")
                     ?? throw new InvalidOperationException("Prompt 'project_intro_generation' niet gevonden.");

        return prompt.PromptText.Replace("{projectName}", projectName);
    }

    public string BuildSurveyGenerationPrompt(string description, int questionAmount)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("survey_generation") ??
                     throw new InvalidOperationException("Prompt 'survey_generation' niet gevonden.");
        return prompt.PromptText
            .Replace("{{description}}", description)
            .Replace("{{questionAmount}}", questionAmount.ToString());
    }

    public string BuildIdeaImprovementPrompt(string title, string text, string language = "")
    {
        var prompt = _aiRepository.ReadAiPromptByKey("idea_improvement")
                     ?? throw new InvalidOperationException("Prompt 'idea_improvement' niet gevonden.");

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
        var prompt = _aiRepository.ReadAiPromptByKey("project_trend_summary")
                     ?? throw new InvalidOperationException("Prompt 'project_trend_summary' niet gevonden.");

        return prompt.PromptText.Replace("{{projectData}}", projectData);
    }

    public string BuildOpenQuestionSummaryPrompt(string question, string answers)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("open_question_summary")
                     ?? throw new InvalidOperationException("Prompt 'open_question_summary' niet gevonden.");

        return prompt.PromptText
            .Replace("{{question}}", question)
            .Replace("{{answers}}", answers);
    }


    public string BuildIdeaFollowUpQuestionsPrompt(string title, string text)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("idea_follow_up_questions")
                     ?? throw new InvalidOperationException("Prompt 'idea_follow_up_questions' niet gevonden.");

        return prompt.PromptText
            .Replace("{title}", title)
            .Replace("{text}", text);
    }

    public Task<string> BuildProjectImageGenerationPromptAsync(string title, string description)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("project_image_generation")
                     ?? throw new InvalidOperationException("Prompt 'project_image_generation' niet gevonden.");

        var result = prompt.PromptText
            .Replace("{projectName}", title)
            .Replace("{introduction}", description);

        return Task.FromResult(result);
    }

    public string BuildIdeaSelectionPrompt(string selectionMode, string projectData)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("idea_selection")
                     ?? throw new InvalidOperationException("Prompt 'idea_selection' niet gevonden.");

        return prompt.PromptText
            .Replace("{{selectionMode}}", selectionMode)
            .Replace("{{projectData}}", projectData);
    }
    
    public string BuildIdeaFollowUpSummaryPrompt(string title, string text, string followUpAnswers)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("idea_follow_up_summary")
                     ?? throw new InvalidOperationException("Prompt 'idea_follow_up_summary' niet gevonden.");

        return prompt.PromptText
            .Replace("{title}", title)
            .Replace("{text}", text)
            .Replace("{followUpAnswers}", followUpAnswers);
    }
}