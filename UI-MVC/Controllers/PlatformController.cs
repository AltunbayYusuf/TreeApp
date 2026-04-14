using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
public class PlatformController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}