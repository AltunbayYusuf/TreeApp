using IntegratieProject.BL.Interfaces;
using Microsoft.Extensions.AI;

namespace IntegratieProject.BL.Ai;

public class VertexAiProvider : IAiProvider
{
    private readonly IChatClient _chatClient;

    public VertexAiProvider(IChatClient chatClient)
    {
        _chatClient = chatClient;
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
        return response.Text?.Trim() ?? "";
    }
    
    
}