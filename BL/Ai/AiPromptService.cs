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

        return $"""
                {prompt.PromptText}

                INPUT:
                TITLE:
                {title}

                TEXT:
                {text}

                OVERRIDE:
                Behoud exact de taal van de titel en tekst. Vertaal niet naar Nederlands.
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

    public string BuildIdeaImprovementPrompt(string title, string text)
    {
        var prompt = _aiRepository.ReadAiPromptByKey("idea_improvement")
                     ?? throw new InvalidOperationException("Prompt 'idea_improvement' niet gevonden.");

        return $"""
                {prompt.PromptText
                    .Replace("{title}", title)
                    .Replace("{text}", text)}

                OVERRIDE:
                Behoud exact de taal van de titel en tekst. Vertaal niet naar Nederlands.
                """;
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
}
