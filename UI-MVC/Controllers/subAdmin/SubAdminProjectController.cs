using IntergratieProject.BL.interfaces;
using IntergratieProject.DAL.Identity;
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

    public SubAdminProjectsController(IManager manager, UserManager<ApplicationUser> userManager)
    {
        _manager = manager;
        _userManager = userManager;
    }

    private async Task<IActionResult> ValidateSubplatformAccess(string subplatform)
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

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> ProjectInfo(string subplatform)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null)
        {
            return errorResult;
        }
       
        var vm = new CreateProjectInfoViewModel
        {
            SubplatformSlug = subplatform,
            Type = ProjectType.VerticalScroll
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> CreateIdeation(string subplatform)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null)
        {
            return errorResult;
        }

        var vm = new CreateProjectIdeationViewModel
        {
            SubplatformSlug = subplatform
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveIdeation(string subplatform, CreateProjectIdeationViewModel vm)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null)
        {
            return errorResult;
        }

        if (!ModelState.IsValid)
        {
            vm.SubplatformSlug = subplatform;
            return View("CreateIdeation", vm);
        }

        vm.SubplatformSlug = subplatform;
        TempData["IdeationSavedMessage"] = "Ideation-instellingen zijn opgeslagen in de sessie. Opslag is nog niet gekoppeld aan het projectdomein.";
        return RedirectToAction(nameof(CreateIdeation), new { subplatform });
    }

    [HttpGet]
    public async Task<IActionResult> CreateSurvey(string subplatform)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null)
        {
            return errorResult;
        }

        var vm = new CreateProjecSurveyViewModel
        {
            SubplatformSlug = subplatform
        };

        return View(vm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSurvey(string subplatform, CreateProjecSurveyViewModel vm)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null)
        {
            return errorResult;
        }

        if (!ModelState.IsValid)
        {
            vm.SubplatformSlug = subplatform;
            return View("CreateSurvey", vm);
        }

        vm.SubplatformSlug = subplatform;
        return RedirectToAction(nameof(CreateSurvey), new { subplatform });
    }
}