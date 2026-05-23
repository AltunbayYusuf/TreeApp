using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers;

public class IdeaController : Controller
{
    private readonly ITopicManager _topicManager;
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectManager _projectManager;
    private readonly IUserManager _userManager;


    public IdeaController(ITopicManager topicManager, IIdeaManager ideaManager, IProjectManager projectManager,
        IUserManager userManager)
    {
        _topicManager = topicManager;
        _ideaManager = ideaManager;
        _projectManager = projectManager;
        _userManager = userManager;
    }

    private string Subplatform => HttpContext.Items["subplatform"]?.ToString() ?? "";

    public IActionResult Index(int projectId, int? topicId)
    {
        var subplatform = Subplatform;
        Project project = GetCurrentProject(subplatform, projectId);
        if (project == null) return NotFound();

        if (project.Status != Status.Active)
        {
            return NotFound();
        }

        ViewBag.SubPlatformSlug = subplatform;

        var vm = new IdeasOverviewViewModel()
        {
            Project = project,
            CurrentUserId = GetCurrentUserId(),
            SelectedTopicId = topicId,
            Topics = _topicManager.GetTopicsByProject(project),
            Ideas = _ideaManager.GetIdeasByProject(project, topicId),
            ReactionEmojis = GetReactionEmojis(project.ReactionEmojiGroup)
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create(int projectId)
    {
        var subplatform = Subplatform;
        Project project = GetCurrentProject(subplatform, projectId);
        if (project == null) return NotFound();

        if (project.Status != Status.Active)
        {
            return NotFound();
        }

        ViewBag.SubPlatformSlug = subplatform;

        var vm = new IdeasOverviewViewModel
        {
            Project = project,
            CurrentUserId = GetCurrentUserId(),
            Topics = _topicManager.GetTopicsByProject(project),
            Ideas = _ideaManager.GetIdeasByProject(project),
            ReactionEmojis = GetReactionEmojis(project.ReactionEmojiGroup)
        };

        return View(vm);
    }

    private Project GetCurrentProject(string subplatform, int projectId)
    {
        return _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);
    }

    private static IEnumerable<string> GetReactionEmojis(string reactionEmojiGroup)
    {
        var reactionEmojis = (reactionEmojiGroup ?? "👍,❤️")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(emoji => !string.IsNullOrWhiteSpace(emoji))
            .Distinct()
            .ToList();

        return reactionEmojis.Any() ? reactionEmojis : new List<string> { "👍", "❤️" };
    }

    private int? GetCurrentUserId()
    {
        var userGuid = Request.Cookies["UserIdentifier"];
        if (string.IsNullOrWhiteSpace(userGuid))
        {
            return null;
        }

        return _userManager.GetUser(userGuid)?.Id;
    }
}