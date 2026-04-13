using IntergratieProject.BL;
using IntergratieProject.DAL.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace IntergratieProject.UI.MVC.Controllers;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubPlatformController : Controller
{
    private readonly IManager _manager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubPlatformController(IManager manager, UserManager<ApplicationUser> userManager)
    {
        _manager = manager;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform)) return Content("subplatform is NULL");

        var currentSubPlatform = _manager.GetSubPlatformBySlug(subplatform);
        if (currentSubPlatform == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Redirect("/Identity/Account/Login");
        }

        if (!string.Equals(user.SubPlatformSlug, subplatform, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        ViewBag.SubPlatformSlug = subplatform;
        return View(currentSubPlatform);
    }
}