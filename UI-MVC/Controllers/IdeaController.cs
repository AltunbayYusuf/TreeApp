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


    public IActionResult Index(int projectId,int? topicId)
    {
        Project project = GetCurrentProject(projectId);
        if (project == null) return NotFound();
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
    public IActionResult Create(int projectId)
    {
        Project project = GetCurrentProject(projectId);
        if (project == null) return NotFound();

        var vm = new IdeasOverviewViewModel
        {
            Project = project,
            Topics = _manager.GetTopicsByProject(project),
            Ideas = _manager.GetIdeasByProject(project)
        };

        return View(vm);
    }

    

    private Project? GetCurrentProject(int projectId)
    {
        return _manager.GetProject(projectId);
    }
}