using IntergratieProject.Domain.Ai;

namespace IntergratieProject.BL;

public class Manager : IManager
{
    private readonly IAiService _aiService;

    public Manager(IAiService aiService)
    {
        _aiService = aiService;
    }

    public async Task<string> AskAiForIdea(string idea)
    {
        var prompt = $"Analyseer dit idee en geef feedback:\n{idea}";

        return await _aiService.GenerateAsync(prompt, FeatureType.IdeaSelection);
    }
}