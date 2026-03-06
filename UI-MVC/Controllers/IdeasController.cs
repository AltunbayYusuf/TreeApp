using IntergratieProject.BL;
using IntergratieProject.Domain.project;
using IntergratieProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class IdeasController : Controller
{
    private IManager _manager;

    public IdeasController(IManager manager)
    {
        _manager = manager;
    }

    public IActionResult Index(int? topicId)
    {
        Project project = getCurrentProject();

        var vm = new IdeasOverviewViewModel()
        {
            Project = project,
            SelectedTopicId = topicId,
            Topics = _manager.GetTopicsByProject(project),
            Ideas = _manager.GetIdeasByProject(project, topicId)
        };
        return View(vm);
    }

    private Project getCurrentProject()
    {
        return new Project()
        {
            Id = 1
        };
    }
}