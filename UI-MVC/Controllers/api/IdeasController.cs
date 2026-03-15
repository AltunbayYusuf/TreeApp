using IntergratieProject.BL;
using IntergratieProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.api;

[ApiController]
[Route("api/[controller]")]
public class IdeasController : ControllerBase
{
    private readonly IManager _manager;

    public IdeasController(IManager manager)
    {
        _manager = manager;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitIdea(SubmitIdeaViewModel vm)
    {
        if (vm.TopicId <= 0)
        {
            return BadRequest(new
            {
                ok = false,
                message = "Kies eerst een topic."
            });
        }

        if (string.IsNullOrWhiteSpace(vm.Text))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Tekst mag niet leeg zijn."
            });
        }

        var result = await _manager.SubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text);

        if (result.IsToxic)
        {
            return Ok(new
            {
                ok = true,
                saved = false,
                isToxic = true,
                warning = "Deze tekst bevat toxische inhoud en werd niet verzonden.",
                explanation = result.Explanation,
                suggestedText = result.SuggestedText
            });
        }

        return Ok(new
        {
            ok = true,
            saved = true,
            isToxic = false,
            message = "Idee succesvol verstuurd."
        });
    }

    [HttpPost("force")]
    public async Task<IActionResult> ForceSubmitIdea([FromBody] SubmitIdeaViewModel vm)
    {
        if (vm.TopicId <= 0)
        {
            return BadRequest(new
            {
                ok = false,
                message = "Kies eerst een topic."
            });
        }

        if (string.IsNullOrWhiteSpace(vm.Text))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Beschrijving mag niet leeg zijn."
            });
        }

        await _manager.ForceSubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text);

        return Ok(new
        {
            ok = true,
            saved = true,
            isToxic = false,
            message = "Idee doorgestuurd voor moderatie."
        });
    }
}