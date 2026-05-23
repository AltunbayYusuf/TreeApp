using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Domain.users;
using IntegratieProject.UI.MVC.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.Admin;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserManager _userManager;

    public AdminController(ILogger<HomeController> logger, IUserManager userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }
    
    public IActionResult Index()
    {
        var generalAdmin = _userManager.GetGeneralAdmin();

        if (generalAdmin == null)
        {
            return Content("Geen general admin gevonden.");
        }

        return View(ToDashboardViewModel(generalAdmin));
    }

    private static AdminDashboardViewModel ToDashboardViewModel(GeneralAdmin generalAdmin)
    {
        var subPlatforms = generalAdmin.Platform.SubPlatforms.ToList();
        var subAdmins = generalAdmin.SubAdmins.ToList();

        return new AdminDashboardViewModel
        {
            SubPlatformCount = subPlatforms.Count,
            SubAdminCount = subAdmins.Count,
            ProjectCount = subPlatforms.Sum(sp => sp.Projects?.Count ?? 0),
            ParticipantCount = subPlatforms.Sum(sp => sp.Projects?.Sum(p => p.SurveyResponses?.Count ?? 0) ?? 0),
            SubPlatforms = subPlatforms.Select(sp => new AdminDashboardSubPlatformViewModel
            {
                Id = sp.Id,
                CompanyName = sp.CompanyName,
                Initial = string.IsNullOrWhiteSpace(sp.CompanyName) ? "?" : sp.CompanyName[..1].ToUpper(),
                SubAdminCount = subAdmins.Count(sa => sa.SubPlatform.Id == sp.Id),
                ProjectCount = sp.Projects?.Count ?? 0,
                ParticipantCount = sp.Projects?.Sum(p => p.SurveyResponses?.Count ?? 0) ?? 0
            }).ToList()
        };
    }
}
