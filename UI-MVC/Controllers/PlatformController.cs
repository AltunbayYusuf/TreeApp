using IntegratieProject.BL.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
public class PlatformController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IManager _manager;

    public PlatformController(ILogger<HomeController> logger, IManager manager)
    {
        _logger = logger;
        _manager = manager;
    }
    
    public IActionResult Index()
    {
        return View();
    }
}