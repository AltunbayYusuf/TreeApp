using IntergratieProject.BL;
using IntergratieProject.UI.MVC.Models;
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
    public async Task<IActionResult> SubmitIdea([FromBody]SubmitIdeaViewModel vm)
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

        try
        {
            var result = await _manager.SubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text);

        if (result.IsToxic)
        {
            return Ok(new
            {
                ok = true,
                saved = false,
                isToxic = true,
                aiUnavailable = false,
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
            aiUnavailable = false,
            message = "Idee succesvol verstuurd."
        });
            
        }
        catch (Exception)
        {
            await _manager.ForceSubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text);

            return Ok(new
            {
                ok = true,
                saved = true,
                isToxic = false,
                aiUnavailable = true,
                message = "Je idee werd opgeslagen en doorgestuurd voor moderatie omdat de AI-controle tijdelijk niet beschikbaar was."
            });
        }
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

        try
        {
            
        await _manager.ForceSubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text);

            return Ok(new
            {
            ok = true,
            saved = true,
            isToxic = false,
            aiUnavailable = false,
            message = "Idee doorgestuurd voor moderatie."
            });
        }
        catch (Exception)
        {
            return StatusCode(503, new
            {
                ok = false,
                isToxic = false,
                aiUnavailable = true,
                message = "De dienst is tijdelijk niet beschikbaar. Probeer het straks opnieuw."
            });
        }
    }
}