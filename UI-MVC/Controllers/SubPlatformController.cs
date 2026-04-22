using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace IntegratieProject.UI.MVC.Controllers;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubPlatformController : Controller
{
    private readonly ISubplatformManager _subplatformManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubPlatformController(ISubplatformManager subplatformManager, UserManager<ApplicationUser> userManager)
    {
        _subplatformManager = subplatformManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform)) return Content("subplatform is NULL");

        var currentSubPlatform = _subplatformManager.GetSubPlatformBySlug(subplatform);
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