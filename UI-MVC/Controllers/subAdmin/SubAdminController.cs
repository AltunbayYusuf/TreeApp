using IntergratieProject.BL;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

[Authorize(Roles = "SubAdmin")]
public class SubAdminController : Controller
{
    private readonly IManager _manager;

    public SubAdminController(IManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public IActionResult Index(string subplatform)
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