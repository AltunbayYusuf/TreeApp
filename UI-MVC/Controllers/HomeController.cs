using System.Diagnostics;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProjectManager _projectManager;

    public HomeController(ILogger<HomeController> logger,  IProjectManager projectManager)
    {
        _logger = logger;
        _projectManager = projectManager;
    }

    private string Subplatform => HttpContext.Items["subplatform"]?.ToString() ?? "";

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy(int? projectId)
    {
        if (projectId.HasValue)
        {
            var project = _projectManager.GetProjectBySubPlatformAndProjectId(Subplatform, projectId.Value);
            ViewBag.ProjectFontFamily = project?.FontFamily;
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}