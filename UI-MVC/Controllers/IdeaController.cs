using IntergratieProject.BL;
using IntergratieProject.Domain.project;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class IdeaController : Controller
{
    private readonly IManager _manager;

    public IdeaController(IManager manager)
    {
        _manager = manager;
    }

    public IActionResult Index(string subplatform, int projectId, int? topicId)
    {
        Project project = GetCurrentProject(subplatform, projectId);
        if (project == null) return NotFound();

        ViewBag.SubPlatformSlug = subplatform;

        var vm = new IdeasOverviewViewModel()
        {
            Project = project,
            SelectedTopicId = topicId,
            Topics = _manager.GetTopicsByProject(project),
            Ideas = _manager.GetIdeasByProject(project, topicId)
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create(string subplatform, int projectId)
    {
        Project project = GetCurrentProject(subplatform, projectId);
        if (project == null) return NotFound();

        ViewBag.SubPlatformSlug = subplatform;

        var vm = new IdeasOverviewViewModel
        {
            Project = project,
            Topics = _manager.GetTopicsByProject(project),
            Ideas = _manager.GetIdeasByProject(project)
        };

        return View(vm);
    }

    private Project? GetCurrentProject(string subplatform, int projectId)
    {
        return _manager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);
    }
}