using IntergratieProject.BL;
using IntergratieProject.DAL.Identity;
using IntergratieProject.Domain.project;
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
        var ideasInReview = _manager.GetIdeasInReviewBySubPlatform(subPlatformEntity.Id);
        var reactionsInReview = _manager.GetReactionsInReviewBySubPlatform(subPlatformEntity.Id);

        var vm = new SubAdminDashboardViewModel
        {
            SubPlatformId = subPlatformEntity.Id,
            SubPlatformName = subPlatformEntity.CompanyName,
            Slug = subPlatformEntity.Slug,
            Projects = projects.Select(p => new ProjectSummaryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Status = p.Status.ToString(),
                Form = p.Type == 0 ? "Scroll" : "Chat",
                ParticipantsCount = p.SurveyResponses != null
                    ? p.SurveyResponses.Count
                    : 0,
                IdeasCount = p.Topics != null
                    ? p.Topics.SelectMany(t => t.Ideas).Count()
                    : 0,

                ReleaseDate = p.ReleaseDate
                Name = p.Introduction,
                Status = p.Status.ToString()
            }).ToList(),

            IdeasReviews = ideasInReview.Select(i => new IdeaReviewSummaryViewModel
            {
                Id = i.Id,
                Title = i.Title ?? "",
                Text = i.Text ?? "",
                TopicTheme = i.Topic?.Theme ?? "",
                ProjectName = i.Topic?.Project?.Introduction ?? "",
                ModerationStatus = i.ModerationStatus.ToString()
            }).ToList(),

            ReactionsReviews = reactionsInReview.Select(r => new ReactionReviewSummaryViewModel
            {
                Id = r.Id,
                Text = r.Text ?? "",
                Emoji = r.Emoji ?? "",
                IdeaTitle = r.Idea?.Title ?? "",
                ProjectName = r.Idea?.Topic?.Project?.Introduction ?? "",
                ModerationStatus = r.ModerationStatus.ToString()
            }).ToList()
        };

        return View(vm);
    }
    
    [HttpPost]
    public IActionResult UpdateStatus(int projectId, string status, string subplatform)
    {
        var project = _manager.GetProject(projectId);

        if (project == null)
            return NotFound();

        project.Status = Enum.Parse<Status>(status);

        _manager.UpdateProject(project);

        return RedirectToAction("Index", new { subplatform });
    }
}