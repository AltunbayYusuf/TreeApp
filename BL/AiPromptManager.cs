using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL;

public class AiPromptManager : IAiPromptManager
{
    private readonly IAiRepository _aiRepository;

    public AiPromptManager(IAiRepository aiRepository)
    {
        _aiRepository = aiRepository;
    }

    public IList<AiPrompt> GetAllPrompts()
    {
        return _aiRepository.ReadAllAiPrompts();
    }

    public void UpdatePrompt(int id, string promptText)
    {
        if (string.IsNullOrWhiteSpace(promptText))
        {
            throw new ArgumentException("Prompt mag niet leeg zijn.");
        }

        var prompt = _aiRepository.ReadAiPromptById(id);

        if (prompt == null)
        {
            throw new InvalidOperationException("Prompt niet gevonden.");
        }

        prompt.PromptText = promptText;
        _aiRepository.UpdateAiPrompt(prompt);
    }
}