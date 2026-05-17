using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.Identity;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminController : Controller
{
    private readonly ISubplatformManager _subplatformManager;
    private readonly IReactionManager _reactionManager;
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectManager _projectManager;

    private readonly UserManager<ApplicationUser> _userManager;

    public SubAdminController(ISubplatformManager subplatformManager, UserManager<ApplicationUser> userManager,IReactionManager reactionManager,IIdeaManager ideaManager,IProjectManager projectManager)
    {
        _subplatformManager = subplatformManager;
        _userManager = userManager;
        _reactionManager = reactionManager;
        _ideaManager = ideaManager;
        _projectManager = projectManager;
    }

    private string Subplatform
    {
        get
        {
            var fromRoute = RouteData.Values["subplatform"]?.ToString();
            return !string.IsNullOrWhiteSpace(fromRoute) ? fromRoute : (HttpContext.Items["subplatform"]?.ToString() ?? "");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var subplatform = Subplatform;
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return NotFound();
        }

        var subPlatformEntity = _subplatformManager.GetSubPlatformBySlug(subplatform);

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

        var projects = _projectManager.GetProjectsBySubPlatform(subPlatformEntity.Id);
        var ideasInReview = _ideaManager.GetIdeasInReviewBySubPlatform(subPlatformEntity.Id);
        var reactionsInReview = _reactionManager.GetReactionsInReviewBySubPlatform(subPlatformEntity.Id);

        
        var projectSummaries = projects.Select(p => new ProjectSummaryViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Status = p.Status.ToString(),
            Form = p.Type == 0 ? "Scroll" : "Chat",
            ParticipantsCount = p.SurveyResponses != null ? p.SurveyResponses.Count : 0,
            IdeasCount = p.Topics != null ? p.Topics.SelectMany(t => t.Ideas).Count() : 0,
            ReleaseDate = p.ReleaseDate
        }).ToList();

        var vm = new SubAdminDashboardViewModel
        {
            SubPlatformId = subPlatformEntity.Id,
            SubPlatformName = subPlatformEntity.CompanyName,
            Slug = subPlatformEntity.Slug,

            TotalProjects = projectSummaries.Count,
            ActiveProjects = projectSummaries.Count(p => p.Status == "Active"),
            ParticipantsCount = projectSummaries.Sum(p => p.ParticipantsCount),
            TotalIdeas = projectSummaries.Sum(p => p.IdeasCount),

            Projects = projectSummaries
        };

        return View(vm);
    }
    
    [HttpPost]
    public IActionResult UpdateStatus(int projectId, string status)
    {
        var subplatform = Subplatform;
        var project = _projectManager.GetProject(projectId);

        if (project == null)
            return NotFound();

        if (!Enum.TryParse<Status>(status, true, out var newStatus))
            return BadRequest();

        var currentStatus = project.Status;

        var isValidTransition =
            (currentStatus == Status.Draft && newStatus == Status.Active) ||
            (currentStatus == Status.Active && newStatus == Status.Archived);

        if (!isValidTransition)
        {
            TempData["Error"] = "Deze statuswijziging is niet toegestaan.";
            return RedirectToAction("Index");
        }

        project.Status = newStatus;
        _projectManager.UpdateProject(project);

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int projectId)
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

        var project = _projectManager.GetProject(projectId);
        if (project == null)
            return NotFound();

        if (project.SubPlatformId != subPlatformEntity.Id)
            return Forbid();

        _projectManager.DeleteProject(projectId);

        TempData["Success"] = "Project werd succesvol verwijderd.";

        return RedirectToAction("Index");
    }
}