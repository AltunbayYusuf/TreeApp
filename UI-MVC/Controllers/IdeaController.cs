using IntergratieProject.BL.interfaces;
using IntergratieProject.Domain.project;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class IdeaController : Controller
{
    private readonly IManager _manager;
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectManager _projectManager;



    public IdeaController(IManager manager,IIdeaManager ideaManager,IProjectManager projectManager)
    {
        _manager = manager;
        _ideaManager = ideaManager;
        _projectManager = projectManager;
    }
    
    public IActionResult Index(string subplatform, int projectId, int? topicId)
    {
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
            SelectedTopicId = topicId,
            Topics = _manager.GetTopicsByProject(project),
            Ideas = _ideaManager.GetIdeasByProject(project, topicId)
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create(string subplatform, int projectId)
    {
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
            Topics = _manager.GetTopicsByProject(project),
            Ideas = _ideaManager.GetIdeasByProject(project)
        };

        return View(vm);
    }

    private Project? GetCurrentProject(string subplatform, int projectId)
    {
        return _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);
    }
}