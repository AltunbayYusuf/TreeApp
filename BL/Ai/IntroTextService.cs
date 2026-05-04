using IntegratieProject.BL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class IntroTextService : IIntroTextService
{
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _promptService;

    public IntroTextService(IAiProvider aiProvider, IAiPromptService promptService)
    {
        _aiProvider = aiProvider;
        _promptService = promptService;
    }

    public async Task<string> GenerateIntroAsync(string projectName)
    {
        var prompt = _promptService.BuildProjectIntroPrompt(projectName);
        return await _aiProvider.GenerateAsync(prompt);
    }
}