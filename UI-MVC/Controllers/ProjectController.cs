using IntergratieProject.BL;
using IntergratieProject.UI.MVC.Routing;
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
        var normalizedSubplatform = SubPlatformRouteHelper.Normalize(subplatform);
        var project = _manager.GetProjectBySubPlatformAndProjectId(normalizedSubplatform, id);

        if (project == null)
        {
            return NotFound();
        }

        ViewBag.SubPlatformSlug = SubPlatformRouteHelper.ToPublicSlug(normalizedSubplatform);
        return View(project);
    }
    
    
}
