using IntergratieProject.BL;
using IntergratieProject.Domain.ideas;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class ReactionController : Controller
{
    private readonly IManager _manager;

    public ReactionController(IManager manager)
    {
        _manager = manager;
    }

    [HttpPost]
    public async Task<IActionResult> Add(int ideaId, string emoji, string text)
    {
        if (ideaId <= 0)
        {
            return Json(new
            {
                ok = false,
                message = "Ongeldig idee."
            });
        }

        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(emoji))
        {
            return Json(new
            {
                ok = false,
                message = "Reactie mag niet leeg zijn."
            });
        }

        var result = await _manager.AddReaction(ideaId, emoji, text);

        if (result.IsToxic)
        {
            return Json(new
            {
                ok = true,
                saved = false,
                isToxic = true,
                warning = "Deze reactie bevat toxische inhoud en werd niet verzonden.",
                explanation = result.Explanation,
                suggestedText = result.SuggestedText
            });
        }

        return Json(new
        {
            ok = true,
            saved = true,
            isToxic = false,
            message = "Reactie succesvol toegevoegd."
        });
    }
}