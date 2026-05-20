using System.Text;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL.Ai;

public class AiIdeaSelectionService : IAiIdeaSelectionService
{
     private readonly IIdeaRepository _ideaRepository;
    private readonly IAiPromptService _promptService;
    private readonly IAiProvider _aiProvider;
    private readonly IAiUsageRepository _aiUsageRepository;
    private readonly IAiRepository _aiRepository;

    private const int MaxIdeas = 40;
    private const int MaxReactionsPerIdea = 5;
    private const int MaxTextLength = 350;

    public AiIdeaSelectionService(
        IIdeaRepository ideaRepository,
        IAiPromptService promptService,
        IAiProvider aiProvider,
        IAiUsageRepository aiUsageRepository,
        IAiRepository aiRepository)
    {
        _ideaRepository = ideaRepository;
        _promptService = promptService;
        _aiProvider = aiProvider;
        _aiUsageRepository = aiUsageRepository;
        _aiRepository = aiRepository;
    }

    public async Task<string> GenerateIdeaSelectionAsync(int projectId, string selectionMode)
    {
        try
        {
            var ideas = _ideaRepository
                .ReadAcceptedIdeasWithReactionsByProjectId(projectId)
                .Take(MaxIdeas)
                .ToList();

            if (!ideas.Any())
            {
                return "[]";
            }

            var ideaCount = ideas.Count;
            var reactionCount = ideas.Sum(i => i.Reactions?.Count() ?? 0);

            var existingSelection = _aiRepository.ReadIdeaSelection(projectId, selectionMode);

            if (existingSelection != null &&
                existingSelection.GeneratedAt >= DateTime.UtcNow.AddDays(-1) &&
                existingSelection.IdeaCountAtGeneration == ideaCount &&
                existingSelection.ReactionCountAtGeneration == reactionCount)
            {
                return existingSelection.ResultJson;
            }

            var projectData = BuildCompactIdeaData(ideas);
            var prompt = _promptService.BuildIdeaSelectionPrompt(selectionMode, projectData);

            var resultJson = await _aiProvider.GenerateAsync(prompt);

            _aiRepository.SaveIdeaSelection(
                projectId,
                selectionMode,
                resultJson,
                ideaCount,
                reactionCount
            );

            _aiUsageRepository.AddUsage(new AiUsage
            {
                Feature = "IdeaSelection",
                Model = "gemini-2.5-flash-lite",
                Success = true
            });

            return resultJson;
        }
        catch (Exception ex)
        {
            _aiUsageRepository.AddUsage(new AiUsage
            {
                Feature = "IdeaSelection",
                Model = "gemini-2.5-flash-lite",
                Success = false,
                ErrorMessage = ex.Message
            });

            return "[]";
        }
    }

    private static string BuildCompactIdeaData(IEnumerable<Idea> ideas)
    {
        var sb = new StringBuilder();

        foreach (var idea in ideas)
        {
            sb.AppendLine($"IDEA_ID: {idea.Id}");
            sb.AppendLine($"TOPIC: {idea.Topic?.Theme}");
            sb.AppendLine($"TITLE: {Limit(idea.Title)}");
            sb.AppendLine($"TEXT: {Limit(idea.Text)}");

            var reactions = idea.Reactions?
                .Where(r => !string.IsNullOrWhiteSpace(r.Text) || !string.IsNullOrWhiteSpace(r.Emoji))
                .Take(MaxReactionsPerIdea)
                .ToList() ?? new List<Reaction>();

            if (reactions.Any())
            {
                sb.AppendLine("REACTIONS:");

                foreach (var reaction in reactions)
                {
                    if (!string.IsNullOrWhiteSpace(reaction.Text))
                        sb.AppendLine($"- {Limit(reaction.Text)}");

                    if (!string.IsNullOrWhiteSpace(reaction.Emoji))
                        sb.AppendLine($"- Emoji: {reaction.Emoji}");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string Limit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        value = value.Trim();

        return value.Length <= MaxTextLength
            ? value
            : value[..MaxTextLength] + "...";
    }
}