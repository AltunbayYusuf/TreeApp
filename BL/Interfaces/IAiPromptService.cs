namespace IntegratieProject.BL.Interfaces;

public interface IAiPromptService
{
    string BuildIdeaModerationPrompt(string title, string text);
    string BuildReactionModerationPrompt(string text);
    string BuildProjectIntroPrompt(string projectName);
    string BuildSurveyGenerationPrompt(string description, int questionAmount);
    string BuildIdeaImprovementPrompt(string title, string text, string language = "");
    string BuildProjectTrendSummaryPrompt(string projectData);
    string BuildOpenQuestionSummaryPrompt(string question, string answers);
    string BuildIdeaFollowUpQuestionsPrompt(string title, string text);
    string BuildProjectImageGenerationPrompt(string title, string description);
    string BuildIdeaSelectionPrompt(string selectionMode, string projectData);
    string BuildIdeaFollowUpSummaryPrompt(string title, string text, string followUpAnswers);
}