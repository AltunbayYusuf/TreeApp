using System.Diagnostics;
using IntergratieProject.BL;
using IntergratieProject.Models;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IManager _manager;

    public HomeController(ILogger<HomeController> logger, IManager manager)
    {
        _logger = logger;
        _manager = manager;
    }

    public IActionResult Index(int id = 1)
    {
        var project = _manager.GetProject(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}