// using IntegratieProject.BL.Interfaces;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.Extensions.AI;
//
// namespace IntegratieProject.BL.Ai;
//
// public class ImageGenerationService : IImageGenerationService
// {
//     private readonly IImageGenerator _imageGenerator;
//     private readonly IAiPromptService _promptService;
//     private readonly IWebHostEnvironment _environment;
//
//     public ImageGenerationService(
//         IImageGenerator imageGenerator,
//         IAiPromptService promptService,
//         IWebHostEnvironment environment)
//     {
//         _imageGenerator = imageGenerator;
//         _promptService = promptService;
//         _environment = environment;
//     }
//
//     public async Task<string> GenerateProjectImageAsync(string projectName)
//     {
//         if (string.IsNullOrWhiteSpace(projectName))
//         {
//             throw new ArgumentException("Project name is required.", nameof(projectName));
//         }
//
//         var prompt = _promptService.BuildProjectImagePrompt(projectName);
//
//         var options = new ImageGenerationOptions
//         {
//             MediaType = "image/png",
//             Count = 1
//         };
//
//         var response = await _imageGenerator.GenerateImagesAsync(prompt, options);
//
//         var image = response.Contents.OfType<DataContent>().FirstOrDefault();
//
//         if (image == null)
//         {
//             throw new InvalidOperationException("AI image generation returned no image data.");
//         }
//
//         var folderPath = Path.Combine(_environment.WebRootPath, "generated-images");
//         Directory.CreateDirectory(folderPath);
//
//         var fileName = $"project-{Guid.NewGuid():N}.png";
//         var filePath = Path.Combine(folderPath, fileName);
//
//         await File.WriteAllBytesAsync(filePath, image.Data.ToArray());
//
//         return $"/generated-images/{fileName}";
//     }
// }