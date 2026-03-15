using IntergratieProject.BL;
using IntergratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.api;

[ApiController]
[Route("api/[controller]")]
public class ReactionsController : ControllerBase
{
    private readonly IManager _manager;

    public ReactionsController(IManager manager)
    {
        _manager = manager;
    }

    [HttpPost]
    public async Task<ActionResult<ReactionResultDto>> AddReaction(NewReactionDto newReactionDto)
    {
        if (!newReactionDto.IdeaId.HasValue || newReactionDto.IdeaId.Value <= 0)
        {
            return BadRequest(new ReactionResultDto
            {
                Ok = false,
                Saved = false,
                IsToxic = false,
                Message = "Ongeldig idee."
            });
        }

        if (string.IsNullOrWhiteSpace(newReactionDto.Emoji) && string.IsNullOrWhiteSpace(newReactionDto.Text))
        {
            return BadRequest(new ReactionResultDto
            {
                Ok = false,
                Saved = false,
                IsToxic = false,
                Message = "Geef een emoji of tekst in."
            });
        }

        try
        {
            var result = await _manager.AddReaction(
                newReactionDto.IdeaId.Value,
                newReactionDto.Emoji,
                newReactionDto.Text
            );

            if (result.IsToxic)
            {
                return Ok(new ReactionResultDto
                {
                    Ok = true,
                    Saved = false,
                    IsToxic = true,
                    Warning = "Deze reactie bevat toxische inhoud en werd niet verzonden.",
                    Explanation = result.Explanation,
                    SuggestedText = result.SuggestedText
                });
            }

            return Ok(new ReactionResultDto
            {
                Ok = true,
                Saved = true,
                IsToxic = false,
                Message = result.Explanation
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ReactionResultDto
            {
                Ok = false,
                Saved = false,
                IsToxic = false,
                Message = ex.Message
            });
        }
    }
}