using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.Ef;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.AI;

namespace IntegratieProject.BL.Ai;

public class ImageGenerationService : IImageGenerationService
{
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _aiPromptService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ImageGenerationService(
        IAiProvider aiProvider,
        IAiPromptService aiPromptService,
        IWebHostEnvironment webHostEnvironment)
    {
        _aiProvider = aiProvider;
        _aiPromptService = aiPromptService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> GenerateProjectImageAsync(string title, string description)
    {
        var prompt = await _aiPromptService.BuildProjectImageGenerationPromptAsync(
            title,
            description
        );

        var imageBytes = await _aiProvider.GenerateImageAsync(prompt);

        var folderPath = Path.Combine(
            _webHostEnvironment.WebRootPath,
            "images",
            "generated-projects"
        );

        Directory.CreateDirectory(folderPath);

        var fileName = $"project-{Guid.NewGuid():N}.png";

        var filePath = Path.Combine(folderPath, fileName);

        await File.WriteAllBytesAsync(filePath, imageBytes);

        return $"/images/generated-projects/{fileName}";
    }
}