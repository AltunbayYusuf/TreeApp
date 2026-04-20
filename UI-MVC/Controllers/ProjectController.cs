using IntergratieProject.BL;
using IntergratieProject.Domain.project;
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
    public IActionResult Index(string subplatform, int id)
    {
        var project = _manager.GetProjectBySubPlatformAndProjectId(subplatform, id);

        if (project == null)
        {
            return NotFound();
        }
        if (project.Status != Status.Active)
        {
            return NotFound(); 
        }

        ViewBag.SubPlatformSlug = subplatform;
        return View(project);
    }
    
    public IActionResult RedirectToFirstProject(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return NotFound();
        }

        var firstProject = _manager.GetFirstProjectBySubPlatform(subplatform);

        if (firstProject == null)
        {
            return NotFound();
        }
        
        if (firstProject.Status != Status.Active)
        {
            return NotFound(); 
        }

        return RedirectToAction("Index", new
        {
            subplatform = subplatform,
            id = firstProject.Id
        });
    }
}