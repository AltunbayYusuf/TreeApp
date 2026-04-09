using IntergratieProject.BL;
using IntergratieProject.DAL.Identity;
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

    public async Task<IActionResult> Index(string slug)
    {
        var subplatform = _manager.GetSubPlatformBySlug(slug);

        if (subplatform == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user.SubPlatformSlug != slug)
        {
            return Forbid(); // gebruiker probeert ander platform te openen
        }

        return View(subplatform);
    }
}

