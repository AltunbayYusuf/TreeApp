using IntergratieProject.BL;
using IntergratieProject.DAL.Identity;
using IntergratieProject.UI.MVC.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace IntergratieProject.UI.MVC.Controllers;

[Authorize(Roles = "SubAdmin")]
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
        var normalizedSubplatform = SubPlatformRouteHelper.Normalize(subplatform);
        if (string.IsNullOrWhiteSpace(normalizedSubplatform)) return Content("subplatform is NULL");

        var currentSubPlatform = _manager.GetSubPlatformBySlug(normalizedSubplatform);
        if (currentSubPlatform == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Redirect("/Identity/Account/Login");
        }

        if (!string.Equals(user.SubPlatformSlug, normalizedSubplatform, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        ViewBag.SubPlatformSlug = SubPlatformRouteHelper.ToPublicSlug(normalizedSubplatform);
        return View(currentSubPlatform);
    }
}
