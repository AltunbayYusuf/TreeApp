using System.Text.Json;
using IntergratieProject.DAL;
using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;

namespace IntergratieProject.BL;

public class Manager : IManager
{
    private readonly IAiService _aiService;
    private readonly IIdeaRepository _repository;
    

    public Manager(IAiService aiService, IIdeaRepository repository)
    {
        _aiService = aiService;
        _repository = repository;
    }
    

    public async Task<string> AskAiForIdea(string idea)
    {
        var prompt = $"Analyseer dit idee en geef feedback:\n{idea}";

        return await _aiService.GenerateAsync(prompt, FeatureType.IdeaSelection);
    }
    
    public async Task<(bool IsToxic, string SuggestedText, string Explanation)> ModerateTextAsync(string input)
    {
        var prompt =
            """
            Je bent een moderatie-assistent voor een jongerenplatform.
            Antwoord ALLEEN met een JSON object. Geen markdown, geen code fences, geen extra tekst.

            JSON schema:
            {"isToxic":true/false,"explanation":"...","suggestedText":"..."}

            Regels:
            - isToxic=true bij schelden, haatspraak, bedreiging, intimidatie, vernedering
            - suggestedText: respectvolle herformulering met dezelfde bedoeling (leeg als niet toxisch)

            INPUT:
            """ + input;

        var aiText = await _aiService.GenerateAsync(prompt, FeatureType.Moderation);

        // strip ```json fences als die er zijn
        var cleaned = aiText.Trim();
        var firstBrace = cleaned.IndexOf('{');
        var lastBrace = cleaned.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        try
        {
            using var doc = JsonDocument.Parse(cleaned);

            bool isToxic = doc.RootElement.GetProperty("isToxic").GetBoolean();
            string explanation = doc.RootElement.GetProperty("explanation").GetString() ?? "";
            string suggestedText = doc.RootElement.GetProperty("suggestedText").GetString() ?? "";

            return (isToxic, suggestedText, explanation);
        }
        catch (Exception ex)
        {
            // AI faalde / output niet parsebaar => GEEN toxic claim
            return (true, "", $"Moderation check failed: {ex.Message}. Raw: {aiText}");        }
    }

    public IEnumerable<Topic> GetTopicsByProject(Project project)
    {
        return _repository.ReadTopicsByProject(project);
    }

    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null)
    {
        if (topicId.HasValue)
        {
            return _repository.ReadIdeasByTopic(project, topicId.Value);
        }

        return _repository.ReadIdeasByProject(project);
    }

    

   

  
}
    
    