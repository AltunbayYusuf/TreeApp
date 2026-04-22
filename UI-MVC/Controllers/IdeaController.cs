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



    public IdeaController(ITopicManager topicManager, IIdeaManager ideaManager,IProjectManager projectManager)
    {
        _topicManager = topicManager;
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
            Topics = _topicManager.GetTopicsByProject(project),
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
            Topics = _topicManager.GetTopicsByProject(project),
            Ideas = _ideaManager.GetIdeasByProject(project)
        };

        return View(vm);
    }

    private Project? GetCurrentProject(string subplatform, int projectId)
    {
        return _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);
    }
}