using IntergratieProject.BL;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = "SubAdmin")]
public class SubAdminController
    : Controller
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
    
}