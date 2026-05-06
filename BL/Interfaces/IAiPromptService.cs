namespace IntegratieProject.BL.Interfaces;

public interface IAiPromptService
{
    string BuildIdeaModerationPrompt(string title, string text);
    string BuildReactionModerationPrompt(string text);
    string BuildProjectImagePrompt(string projectName);
    string BuildProjectIntroPrompt(string projectName);
    string BuildSurveyGenerationPrompt(string description, int questionAmount);
    string BuildIdeaImprovementPrompt(string title, string text);
    string BuildProjectTrendSummaryPrompt(string projectData);
    string BuildOpenQuestionSummaryPrompt(string question, string answers);
}