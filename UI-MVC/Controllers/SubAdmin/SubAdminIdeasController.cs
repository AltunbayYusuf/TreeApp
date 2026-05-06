using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.Identity;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace IntegratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminIdeasController : Controller
{
    private readonly ISubplatformManager _subplatformManager;
    private readonly IProjectManager _projectManager;
    private readonly IIdeaManager _ideaManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubAdminIdeasController(ISubplatformManager subplatformManager, IProjectManager projectManager,
        IIdeaManager ideaManager, UserManager<ApplicationUser> userManager)
    {
        _subplatformManager = subplatformManager;
        _projectManager = projectManager;
        _ideaManager = ideaManager;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string subplatform)
    {
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

    [HttpGet]
    public async Task<IActionResult> ExportCsv(string subplatform)
    {
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

        var ideas = _ideaManager.GetIdeasBySubPlatform(subPlatformEntity.Id);

        var csv = new StringBuilder();
        csv.AppendLine("Id,Titel,Inhoud,Status,Topic,Project,Email,AantalReacties");

        foreach (var idea in ideas)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(idea.Id.ToString()),
                EscapeCsv(string.IsNullOrWhiteSpace(idea.Title) ? "Zonder titel" : idea.Title),
                EscapeCsv(idea.Text ?? string.Empty),
                EscapeCsv(idea.ModerationStatus.ToString()),
                EscapeCsv(idea.Topic?.Theme ?? "-"),
                EscapeCsv(idea.Topic?.Project?.Name ?? "-"),
                EscapeCsv(idea.User?.Email ?? string.Empty),
                EscapeCsv((idea.Reactions?.Count() ?? 0).ToString())
            ));
        }

        var fileName = $"ideeen-{subplatform}-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
    }

    private static string EscapeCsv(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
