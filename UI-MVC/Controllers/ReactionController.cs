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
    public IActionResult Add(int ideaId, string emoji, string text)
    {
        _manager.AddReaction(ideaId, emoji, text);
        return RedirectToAction("Index", "Idea");
    }
}