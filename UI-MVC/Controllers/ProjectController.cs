using IntergratieProject.BL;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class ProjectController : Controller
{
    private readonly IManager _manager;

    public ProjectController(IManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public IActionResult Index(int id)
    {
        var project = _manager.GetProject(id);

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }
}