using IntegratieProject.BL.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.Admin;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserManager _userManager;

    public AdminController(ILogger<HomeController> logger, IUserManager userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }
    
    public IActionResult Index()
    {
        var generalAdmin = _userManager.GetGeneralAdmin();

        if (generalAdmin == null)
        {
            return Content("Geen general admin gevonden.");
        }

        return View(generalAdmin);
    }
}