using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.Identity;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminModerationController : Controller
{
    private readonly ISubplatformManager _subplatformManager;
    private readonly IReactionManager _reactionManager;
    private readonly IIdeaManager _ideaManager;

    private readonly UserManager<ApplicationUser> _userManager;

    public SubAdminModerationController(ISubplatformManager subplatformManager, UserManager<ApplicationUser> userManager,IReactionManager reactionManager,IIdeaManager ideaManager)
    {
        _subplatformManager = subplatformManager;
        _userManager = userManager;
        _reactionManager = reactionManager;
        _ideaManager = ideaManager;
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
    public async Task<IActionResult> Index(string filter, string selectedType, int? selectedId)
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

        var activeFilter = (filter ?? "algemeen").ToLower();
        var ideasInReview = _ideaManager.GetIdeasInReviewBySubPlatform(subPlatformEntity.Id);
        var reactionsInReview = _reactionManager.GetReactionsInReviewBySubPlatform(subPlatformEntity.Id);

        var ideaItems = ideasInReview.Select(i => new ModerationQueueItemViewModel()
        {
            Type = "idea",
            Id = i.Id,
            Label = "Idee",
            Title = string.IsNullOrWhiteSpace(i.Title) ? "Zonder titel" : i.Title,
            PreviewText = string.IsNullOrWhiteSpace(i.Text) ? "Geen inhoud" : i.Text,
            ContentText = string.IsNullOrWhiteSpace(i.Text) ? "Geen inhoud" : i.Text,
            ProjectName = i.Topic?.Project?.Name ?? "-",
            TopicTheme = i.Topic?.Theme ?? "-",
            IdeaTitle = "",
            ModerationStatus = i.ModerationStatus.ToString(),
            ModerationReason = "Mogelijk ongepaste taal gedetecteerd door AI"
        });

        var reactionItems = reactionsInReview.Select(r => new ModerationQueueItemViewModel
        {
            Type = "reaction",
            Id = r.Id,
            Label = "Reactie",
            Title = string.IsNullOrWhiteSpace(r.Idea?.Title) ? "Reactie op idee" : r.Idea!.Title,
            PreviewText = !string.IsNullOrWhiteSpace(r.Text)
                ? r.Text
                : (string.IsNullOrWhiteSpace(r.Emoji) ? "Geen inhoud" : r.Emoji),
            ContentText = !string.IsNullOrWhiteSpace(r.Text)
                ? r.Text
                : (string.IsNullOrWhiteSpace(r.Emoji) ? "Geen inhoud" : r.Emoji),
            ProjectName = r.Idea?.Topic?.Project?.Name ?? "-",
            TopicTheme = r.Idea?.Topic?.Theme ?? "-",
            IdeaTitle = r.Idea?.Title ?? "-",
            ModerationStatus = r.ModerationStatus.ToString(),
            ModerationReason = "Mogelijk ongepaste taal gedetecteerd door AI"
        });

        IEnumerable<ModerationQueueItemViewModel> allItems = ideaItems.Concat(reactionItems);
        if (activeFilter == "idee")
        {
            allItems = allItems.Where(x => x.Type == "idea");
        }
        else if (activeFilter == "reactie")
        {
            allItems = allItems.Where(x => x.Type == "reaction");
        }

        var items = allItems.OrderByDescending(x => x.Id).ToList();
        ModerationQueueItemViewModel selectedItem = null;

        if (!string.IsNullOrWhiteSpace(selectedType) && selectedId.HasValue)
        {
            selectedItem = items.FirstOrDefault(x =>
                x.Type.Equals(selectedType, StringComparison.OrdinalIgnoreCase) &&
                x.Id == selectedId.Value);
        }

        selectedItem ??= items.FirstOrDefault();

        foreach (var item in items)
        {
            item.IsSelected = selectedItem != null &&
                              item.Type == selectedItem.Type &&
                              item.Id == selectedItem.Id;
        }

        var vm = new SubAdminModerationViewModel
        {
            ActiveFilter = activeFilter,
            Items = items,
            SelectedItem = selectedItem
        };

        return View(vm);
    }

    [HttpPost]
    public IActionResult ApproveIdea(int ideaId)
    {
        _ideaManager.ApproveIdea(ideaId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult RejectIdea(int ideaId)
    {
        _ideaManager.RejectIdea(ideaId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ApproveReaction(int reactionId)
    {
        _reactionManager.ApproveReaction(reactionId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult RejectReaction(int reactionId)
    {
        _reactionManager.RejectReaction(reactionId);
        return RedirectToAction(nameof(Index));
    }
}