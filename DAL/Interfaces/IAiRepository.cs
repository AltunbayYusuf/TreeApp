using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.DAL.Interfaces;

public interface IAiRepository
{
    AiPrompt ReadAiPromptByKey(string key);
}