using IntergratieProject.BL.interfaces;
using IntergratieProject.Domain.users;
using IntergratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.api;

[ApiController]
[Route("api/[controller]")]
public class ReactionsController : ControllerBase
{
    private readonly IReactionManager _reactionManager;
    private readonly IUserManager _userManager;

    public ReactionsController(IReactionManager reactionManager, IUserManager userManager)
    {
        _reactionManager = reactionManager;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<ActionResult<ReactionResultDto>> AddReaction([FromBody] NewReactionDto newReactionDto)
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
            var user = GetOrCreateUser();

            var result = await _reactionManager.AddReaction(
                newReactionDto.IdeaId.Value,
                newReactionDto.Emoji,
                newReactionDto.Text,
                user.Id
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
                AiUnavailable = false,
                Message = result.Explanation
            });
        }
        catch (Exception)
        {
            var user = GetOrCreateUser();

            await _reactionManager.ForceAddReactionAsync(
                newReactionDto.IdeaId.Value,
                newReactionDto.Emoji,
                newReactionDto.Text,
                user.Id
            );

            return Ok(new ReactionResultDto
            {
                Ok = true,
                Saved = true,
                IsToxic = false,
                AiUnavailable = true,
                Message = "Reactie opgeslagen en doorgestuurd voor moderatie omdat AI tijdelijk niet beschikbaar was."
            });
        }
    }

    [HttpPost("force")]
    public async Task<ActionResult<ReactionResultDto>> ForceAddReaction([FromBody] NewReactionDto newReactionDto)
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
            var user = GetOrCreateUser();

            await _reactionManager.ForceAddReactionAsync(
                newReactionDto.IdeaId.Value,
                newReactionDto.Emoji,
                newReactionDto.Text,
                user.Id
            );

            return Ok(new ReactionResultDto
            {
                Ok = true,
                Saved = true,
                IsToxic = false,
                AiUnavailable = false,
                Message = "Reactie doorgestuurd voor moderatie."
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

    private User GetOrCreateUser()
    {
        string? userGuid = Request.Cookies["UserIdentifier"];
        User? user = null;

        if (!string.IsNullOrEmpty(userGuid))
        {
            user = _userManager.GetUser(userGuid);
        }

        if (user == null)
        {
            userGuid = Guid.NewGuid().ToString();

            Response.Cookies.Append("UserIdentifier", userGuid, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddYears(30),
                HttpOnly = true
            });

            user = new User
            {
                CookieIdentifier = userGuid
            };

            _userManager.AddUser(user);
        }

        return user;
    }
}