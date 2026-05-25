using IntegratieProject.BL.Interfaces;
using Microsoft.Extensions.AI;

namespace IntegratieProject.BL.Ai;

public class AiProvider : IAiProvider
{
    private readonly IChatClient _chatClient;
    private readonly IImageGenerator _imageGenerator;

    public AiProvider(
        IChatClient chatClient,
        IImageGenerator imageGenerator)
    {
        _chatClient = chatClient;
        _imageGenerator = imageGenerator;
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, prompt)
        };

        var options = new ChatOptions
        {
            MaxOutputTokens = 2000
        };

        var response = await _chatClient.GetResponseAsync(messages, options);
        return response.Text.Trim();
    }

    public async Task<byte[]> GenerateImageAsync(string prompt)
    {
        var options = new ImageGenerationOptions
        {
            MediaType = "image/png"
        };

        var response = await _imageGenerator.GenerateImagesAsync(prompt, options);

        var image = response.Contents
            .OfType<DataContent>()
            .FirstOrDefault();

        if (image == null)
        {
            throw new InvalidOperationException("De AI heeft geen afbeelding teruggegeven.");
        }

        return image.Data.ToArray();
    }
}
