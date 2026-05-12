using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.DAL.Interfaces;

public interface IAiRepository
{
    AiPrompt ReadAiPromptByKey(string key);
    
    AiOpenQuestionSummary ReadOpenQuestionSummary(int projectId, int questionId);

    void SaveOpenQuestionSummary(
        int projectId,
        int questionId,
        string summary,
        int answerCount,
        int lastAnswerId);
    IList<AiPrompt> ReadAllAiPrompts();
    AiPrompt ReadAiPromptById(int id);
    void UpdateAiPrompt(AiPrompt prompt);
}