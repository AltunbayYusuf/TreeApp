using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.Identity;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminIdeasController : Controller
{
    private readonly ISubplatformManager _subplatformManager;
    private readonly IProjectManager _projectManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubAdminIdeasController( ISubplatformManager subplatformManager,  IProjectManager projectManager,  UserManager<ApplicationUser> userManager)
    {
        _subplatformManager = subplatformManager;
        _projectManager = projectManager;
        _userManager = userManager;
    }

    private string Subplatform => HttpContext.Items["subplatform"]?.ToString() ?? "";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var subplatform = Subplatform;
        if (string.IsNullOrWhiteSpace(subplatform))
            return NotFound();

        var subPlatformEntity = _subplatformManager.GetSubPlatformBySlug(subplatform);
        if (subPlatformEntity == null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Redirect("/Identity/Account/Login");

        if (!string.Equals(user.SubPlatformSlug, subplatform, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var projects = _projectManager.GetProjectsBySubPlatform(subPlatformEntity.Id);

        var projectSummaries = projects.Select(p => new ProjectSummaryViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Status = p.Status.ToString(),
            Form = p.Type == 0 ? "Scroll" : "Chat",
            ParticipantsCount = p.SurveyResponses?.Count ?? 0,
            IdeasCount = p.Topics != null ? p.Topics.SelectMany(t => t.Ideas).Count() : 0,
            ReleaseDate = p.ReleaseDate
        }).ToList();

        var vm = new SubAdminDashboardViewModel
        {
            SubPlatformId = subPlatformEntity.Id,
            SubPlatformName = subPlatformEntity.CompanyName,
            Slug = subPlatformEntity.Slug,
            Projects = projectSummaries
        };

        return View(vm);
    }
}