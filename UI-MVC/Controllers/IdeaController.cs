using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using IntegratieProject.UI.MVC.Services;

namespace IntegratieProject.UI.MVC.Controllers;

public class IdeaController : Controller
{
    private readonly ITopicManager _topicManager;
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectManager _projectManager;
    private readonly IUserManager _userManager;
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptService _aiPromptService;


    public IdeaController(ITopicManager topicManager, IIdeaManager ideaManager,IProjectManager projectManager,IAiProvider aiProvider,
        IAiPromptService aiPromptService, IUserManager userManager)
    {
        _topicManager = topicManager;
        _ideaManager = ideaManager;
        _projectManager = projectManager;
        _userManager = userManager;
        _aiProvider = aiProvider;
        _aiPromptService = aiPromptService;
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
            Ideas = _ideaManager.GetIdeasByProject(project, topicId)
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
            Ideas = _ideaManager.GetIdeasByProject(project)
        };

        return View(vm);
    }

    private Project GetCurrentProject(string subplatform, int projectId)
    {
        return _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);
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