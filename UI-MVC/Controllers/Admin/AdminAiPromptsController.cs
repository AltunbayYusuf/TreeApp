using IntegratieProject.BL.Interfaces;
using IntegratieProject.UI.MVC.Models;
using IntegratieProject.UI.MVC.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.Admin;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
[Route("admin/prompts")]
public class AdminAiPromptsController : Controller
{
    private readonly IAiPromptManager _aiPromptManager;

    public AdminAiPromptsController(IAiPromptManager aiPromptManager)
    {
        _aiPromptManager = aiPromptManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var prompts = _aiPromptManager.GetAllPrompts()
            .Select(prompt => new AiPromptViewModel
            {
                Id = prompt.Id,
                Key = prompt.Key,
                Name = prompt.Name,
                PromptText = prompt.PromptText
            })
            .ToList();

        return View(prompts);
    }

    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int id, string promptText)
    {
        _aiPromptManager.UpdatePrompt(id, promptText);

        TempData["SuccessMessage"] = "Prompt opgeslagen.";
        return RedirectToAction(nameof(Index));
    }
}