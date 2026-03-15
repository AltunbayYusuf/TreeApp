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
        _http = http; // htpp request
        _config = config; // api key
    }

    public async Task<string> GenerateAsync(string prompt, FeatureType feature)
    {
        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini:ApiKey ontbreekt in appsettings/user-secrets.");

        var model = "gemini-3.1-flash-lite-preview";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
       
        // body van de json
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

        var raw = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            // Geef duidelijke fout terug (handig in je chat om te debuggen)
            return $"Gemini error ({(int)response.StatusCode}): {raw}";
        }

        // ✅ Extract de tekst die Gemini genereert
        using var doc = JsonDocument.Parse(raw);

        // candidates[0].content.parts[0].text
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? "";
    }
}