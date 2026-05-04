using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.Admin;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
[Route("[controller]")]
public class AdminStatsController : Controller
{
    private readonly ILogger<AdminStatsController> _logger;
    private readonly IManager _manager;

    public AdminStatsController(
        ILogger<AdminStatsController> logger,
        IManager manager)
    {
        _logger = logger;
        _manager = manager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var subPlatforms = _manager.GetAllSubPlatformsWithAdmins();

        var platforms = subPlatforms.Select(sp => new SubPlatformAdminOverviewViewModel
        {
            SubPlatformName = sp.CompanyName,
            SubPlatformSlug = sp.Slug,
            Admins = sp.SubAdmins.Select(admin => new SubPlatformAdminViewModel
            {
                FullName = admin.Name,
                Email = admin.IdentityUserId
            }).ToList()
        }).ToList();

        var model = new AdminStatsOverviewViewModel
        {
            TotalPlatforms = platforms.Count,
            TotalAdmins = platforms.Sum(p => p.Admins.Count),
            Platforms = platforms
        };

        return View(model);
    }
}