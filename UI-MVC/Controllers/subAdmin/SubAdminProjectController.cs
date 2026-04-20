using IntergratieProject.BL;
using IntergratieProject.DAL.Identity;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminProjectsController : Controller
{
    private readonly IManager _manager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SubAdminProjectsController(IManager manager, UserManager<ApplicationUser> userManager,
        IWebHostEnvironment webHostEnvironment)
    {
        _manager = manager;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> ProjectInfo(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return NotFound();
        }

        var subPlatformEntity = _manager.GetSubPlatformBySlug(subplatform);
        if (subPlatformEntity == null)
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

        var vm = new CreateProjectInfoViewModel
        {
            SubplatformSlug = subplatform,
            Type = ProjectType.VerticalScroll
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> CreateSurvey(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform)) return NotFound();

        var subPlatformEntity = _manager.GetSubPlatformBySlug(subplatform);
        if (subPlatformEntity == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Redirect("/Identity/Account/Login");

        if (!string.Equals(user.SubPlatformSlug, subplatform, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var vm = new CreateProjecSurveyViewModel
        {
            SubplatformSlug = subplatform
        };

        return View(vm);
    }
}