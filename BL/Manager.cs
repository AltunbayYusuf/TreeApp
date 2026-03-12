using System.Text.Json;
using IntergratieProject.DAL;
using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.users;

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

    public void SubmitIdeaFromChatAsync(int topicId, string text)
    {
        
    }
    
    // public void AddReaction(int ideaId, string emoji, string text)
    // {
    //     if (string.IsNullOrWhiteSpace(emoji) && string.IsNullOrWhiteSpace(text))
    //     {
    //         return;
    //     }
    //
    //     _repository.AddReaction(ideaId, emoji, text);
    // }

    public async Task<string> AskAiForIdea(string idea)
    {
        var prompt = $"Analyseer dit idee en geef feedback:\n{idea}";

        return await _aiService.GenerateAsync(prompt, FeatureType.IdeaSelection);
    }
    
    public async Task<ToxicityResult> ModerateTextAsync(string input)
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
    
    //debugging
    Console.WriteLine("RAW AI TEXT:");
    Console.WriteLine(aiText);
    Console.WriteLine("------");

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

        return new ToxicityResult
        {
            IsToxic = isToxic,
            SuggestedText = suggestedText,
            Explanation = explanation
        };
    }
    catch (Exception ex)
    {
        // AI faalde / output niet parsebaar
        return new ToxicityResult
        {
            IsToxic = true,
            SuggestedText = "",
            Explanation = $"Moderation check failed: {ex.Message}. Raw: {aiText}"
        };
    }
}
    public async Task<ToxicityResult> AddReaction(int ideaId, string emoji, string text)
    {
        var idea = _repository.ReadIdeaById(ideaId);

        if (idea == null)
        {
            throw new Exception("Idee niet gevonden");
        }

        if (!string.IsNullOrWhiteSpace(emoji) && string.IsNullOrWhiteSpace(text))
        {
            var reaction = new Reaction
            {
                Idea = idea,
                Emoji = emoji,
                Text = null,
                ModerationStatus = ModerationStatus.Accepted
            };

            _repository.AddReaction(reaction);

            return new ToxicityResult
            {
                IsToxic = false,
                SuggestedText = "",
                Explanation = "Emoji reactie opgeslagen."
            };
        }

        // ALS ER TEKST IS → AI MODERATIE
        var moderation = await ModerateTextAsync(text);

        if (moderation.IsToxic)
        {
            return moderation;
        }

        var textReaction = new Reaction
        {
            Idea = idea,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
            Text = text,
            ModerationStatus = ModerationStatus.Accepted
        };

        _repository.AddReaction(textReaction);

        return new ToxicityResult
        {
            IsToxic = false,
            SuggestedText = "",
            Explanation = "Reactie succesvol opgeslagen."
        };
    }

public async Task ForceSubmitIdeaAsync(int topicId, string title, string text)
{
    var topic = _repository.ReadTopicById(topicId);
    if (topic == null)
    {
        throw new Exception("Topic niet gevonden");
    }

    var idea = new Idea
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title,
        Text = text,
        Topic = topic,
        ModerationStatus = ModerationStatus.InReview
    };

    _repository.AddIdea(idea);
    await Task.CompletedTask;
} 
//     public async Task<(bool IsToxic, string SuggestedText, string Explanation)> ModerateTextAsync(string input)
//     {
//         var prompt =
//             """
//             Je bent een moderatie-assistent voor een jongerenplatform.
//             Antwoord ALLEEN met een JSON object. Geen markdown, geen code fences, geen extra tekst.
//
//             JSON schema:
//             {"isToxic":true/false,"explanation":"...","suggestedText":"..."}
//
//             Regels:
//             - isToxic=true bij schelden, haatspraak, bedreiging, intimidatie, vernedering
//             - suggestedText: respectvolle herformulering met dezelfde bedoeling (leeg als niet toxisch)
//
//             INPUT:
//             """ + input;
//
//         var aiText = await _aiService.GenerateAsync(prompt, FeatureType.Moderation);
//
//         // strip ```json fences als die er zijn
//         var cleaned = aiText.Trim();
//         var firstBrace = cleaned.IndexOf('{');
//         var lastBrace = cleaned.LastIndexOf('}');
//
//         if (firstBrace >= 0 && lastBrace > firstBrace)
//         {
//             cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
//         }
//
//         try
//         {
//             using var doc = JsonDocument.Parse(cleaned);
//
//             bool isToxic = doc.RootElement.GetProperty("isToxic").GetBoolean();
//             string explanation = doc.RootElement.GetProperty("explanation").GetString() ?? "";
//             string suggestedText = doc.RootElement.GetProperty("suggestedText").GetString() ?? "";
//
//             return (isToxic, suggestedText, explanation);
//         }
//         catch (Exception ex)
//         {
//             // AI faalde / output niet parsebaar => GEEN toxic claim
//             return (true, "", $"Moderation check failed: {ex.Message}. Raw: {aiText}");        }
//     }
//     public async Task ForceSubmitIdeaAsync(int topicId, string title, string text)
//     {
//         var topic = _repository.ReadTopicById(topicId);
//         if (topic == null)
//         {
//             throw new Exception("Topic niet gevonden");
//         }
//
//         var idea = new Idea
//         {
//             Title = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title,
//             Text = text,
//             Topic = topic,
//             ModerationStatus = ModerationStatus.InReview
//         };
//
//         _repository.AddIdea(idea);
//         await Task.CompletedTask;
//     }
    public async Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text)
    {
        var topic = _repository.ReadTopicById(topicId);
        if (topic == null)
        {
            throw new Exception("Topic niet gevonden");
        }

        var moderation = await ModerateTextAsync(text);

        if (moderation.IsToxic)
        {
            return moderation;
        }

        var idea = new Idea
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Zonder titel" : title,
            Text = text,
            Topic = topic,
            ModerationStatus = ModerationStatus.Accepted
        };

        _repository.AddIdea(idea);

        return new ToxicityResult
        {
            IsToxic = false,
            SuggestedText = "",
            Explanation = "Idee succesvol opgeslagen."
        };
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

    public IEnumerable<Question> GetAllQuestionsBySection(int sectionId)
    {
        return _repository.ReadAllQuestionsBySection(sectionId);
    }
    public IEnumerable<Question> GetAllQuestions()
    {
        return _repository.ReadAllQuestions();
    }

    public Question GetQuestion(int questionId)
    {
        return _repository.ReadQuestion(questionId);
    }

    public QuestionList GetQuestionListByProject(Project project)
    {
        return _repository.ReadQuestionListByProject(project);
    }

    public Project GetProject(int projectId)
    {
        return _repository.ReadProject(projectId);
    }

    public User GetUser(string cookieId)
    {
        return _repository.ReadUser(cookieId);
    }

    public void AddUser(User user)
    {
        _repository.CreateUser(user);
    }

    public void SaveAnswers(int userId, List<Answer> answers)
    {
        _repository.SaveAnswers(userId, answers);
    }
}