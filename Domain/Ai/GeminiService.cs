using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace IntergratieProject.Domain.Ai;

public class GeminiService : IAiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<string> GenerateAsync(string prompt, FeatureType feature)
    {
        var apiKey = _config["Gemini:ApiKey"];

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(body);
        var response = await _http.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json"));

        var result = await response.Content.ReadAsStringAsync();

        return result;
    }
}