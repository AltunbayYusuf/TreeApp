using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers;

public class ProjectController : Controller
{
    private readonly IProjectManager _projectManager;


    public ProjectController(IProjectManager projectManager)
    {
        _projectManager = projectManager;
    }


    private string Subplatform => HttpContext.Items["subplatform"]?.ToString() ?? "";

    [HttpGet]
    public IActionResult Index(int id = 1)
    {
        var subplatform = Subplatform;
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return NotFound();
        }
        
        var project = _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, id);

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
    
    public IActionResult RedirectToFirstProject()
    {
        var subplatform = Subplatform;
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return NotFound();
        }

        var firstProject = _projectManager.GetFirstProjectBySubPlatform(subplatform);

        if (firstProject == null)
        {
            return NotFound();
        }
        
        if (firstProject.Status != Status.Active)
        {
            return NotFound(); 
        }

        return RedirectToAction("Index", new { id = firstProject.Id });
    }
}