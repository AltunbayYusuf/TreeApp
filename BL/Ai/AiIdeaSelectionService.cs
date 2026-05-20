using System.Text;
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
    private readonly AiUsageService _aiUsageService;
    private readonly IAiRepository _aiRepository;
    private readonly IAiModelConfigurationManager _modelConfigurationManager;
    private readonly IProjectRepository _projectRepository;

    private const int MaxIdeas = 40;
    private const int MaxReactionsPerIdea = 5;
    private const int MaxTextLength = 350;

    public AiIdeaSelectionService(
        IIdeaRepository ideaRepository,
        IAiPromptService promptService,
        IAiProvider aiProvider,
        AiUsageService aiUsageService,
        IAiRepository aiRepository,
        IAiModelConfigurationManager modelConfigurationManager,
        IProjectRepository projectRepository)
    {
        _ideaRepository = ideaRepository;
        _promptService = promptService;
        _aiProvider = aiProvider;
        _aiUsageService = aiUsageService;
        _aiRepository = aiRepository;
        _modelConfigurationManager = modelConfigurationManager;
        _projectRepository = projectRepository;
    }

    public async Task<string> GenerateIdeaSelectionAsync(int projectId, string selectionMode)
    {
        var project = _projectRepository.ReadProject(projectId);
        var subPlatformId = project?.SubPlatformId;

        var config = _modelConfigurationManager.GetActiveConfiguration("IdeaSelection", subPlatformId);
        string prompt = "";

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
            prompt = _promptService.BuildIdeaSelectionPrompt(selectionMode, projectData);

            var resultJson = await _aiProvider.GenerateAsync(prompt);

            _aiRepository.SaveIdeaSelection(
                projectId,
                selectionMode,
                resultJson,
                ideaCount,
                reactionCount
            );

            _aiUsageService.RegisterTextUsage(
                "IdeaSelection",
                config.ModelName,
                prompt,
                resultJson,
                true,
                null,
                subPlatformId
            );

            return resultJson;
        }
        catch (Exception ex)
        {
            _aiUsageService.RegisterTextUsage(
                "IdeaSelection",
                config.ModelName,
                prompt,
                "",
                false,
                ex.Message,
                subPlatformId
            );

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

    private static string Limit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        value = value.Trim();

        return value.Length <= MaxTextLength
            ? value
            : value[..MaxTextLength] + "...";
    }
}