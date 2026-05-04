using IntegratieProject.BL.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IManager _manager;

    public AdminController(ILogger<HomeController> logger, IManager manager)
    {
        _logger = logger;
        _manager = manager;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public IActionResult Index()
    {
        return View();
    }
}