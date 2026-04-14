using IntergratieProject.BL;
using IntergratieProject.DAL.Identity;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminController : Controller
{
    private readonly IManager _manager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubAdminController(IManager manager, UserManager<ApplicationUser> userManager)
    {
        _manager = manager;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string subplatform)
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

        var projects = _manager.GetProjectsBySubPlatform(subPlatformEntity.Id);

        var vm = new SubAdminDashboardViewModel
        {
            SubPlatformId = subPlatformEntity.Id,
            SubPlatformName = subPlatformEntity.CompanyName,
            Slug = subPlatformEntity.Slug,
            Projects = projects.Select(p => new ProjectSummaryViewModel
            {
                Id = p.Id,
                Name = p.Introduction,
                Status = p.Status.ToString()
            }).ToList()
        };

        return View(vm);
    }
}