using IntergratieProject.BL;
using IntergratieProject.DAL.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
public class GeneralAdminController : Controller
{
    private readonly AdminService _adminService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IActionResult Index()

    {
        var identityId = _userManager.GetUserId(User);
        var admin = _adminService.GetByIdentityId(identityId);
        
        return View();
    }
}