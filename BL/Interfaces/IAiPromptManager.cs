using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.BL.Interfaces;

public interface IAiPromptManager
{
    IList<AiPrompt> GetAllPrompts();
    void UpdatePrompt(int id, string promptText);
}