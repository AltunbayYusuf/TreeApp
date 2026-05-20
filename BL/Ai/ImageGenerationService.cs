using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.Ef;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.AI;

namespace IntegratieProject.BL.Ai;

public class ImageGenerationService : IImageGenerationService
{
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _aiPromptService;

    public ImageGenerationService(
        IAiProvider aiProvider,
        IAiPromptService aiPromptService)
    {
        _aiProvider = aiProvider;
        _aiPromptService = aiPromptService;
    }

    public async Task<byte[]> GenerateProjectImageAsync(string title, string description)
    {
        var prompt = await _aiPromptService.BuildProjectImageGenerationPromptAsync(
            title,
            description
        );

        return await _aiProvider.GenerateImageAsync(prompt);
    }
}