using IntegratieProject.BL.Domain.users;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.api;

[ApiController]
[Route("api/[controller]")]
public class IdeasController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;
    private readonly IUserManager _userManager;

    public IdeasController(IIdeaManager ideaManager, IUserManager userManager)
    {
        _ideaManager = ideaManager;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitIdea([FromBody] SubmitIdeaViewModel vm)
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

        var user = SaveContactPreference(vm);
        var result = await _ideaManager.SubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text, user.Id);

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


        var user = SaveContactPreference(vm);
        await _ideaManager.ForceSubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text, user.Id);

        return Ok(new
        {
            ok = true,
            saved = true,
            isToxic = false,
            aiUnavailable = false,
            message = "Idee doorgestuurd voor moderatie."
        });
    }


    private User SaveContactPreference(SubmitIdeaViewModel vm)
    {
        var user = GetOrCreateUser();
        user.Email = vm.ContactOptIn && !string.IsNullOrWhiteSpace(vm.Email) ? vm.Email.Trim() : string.Empty;
        _userManager.UpdateUser(user);
        return user;
    }

    private User GetOrCreateUser()
    {
        var userGuid = Request.Cookies["UserIdentifier"];
        User user = null;

        if (!string.IsNullOrEmpty(userGuid))
        {
            user = _userManager.GetUser(userGuid);
        }

        if (user != null)
        {
            return user;
        }

        userGuid = Guid.NewGuid().ToString();

        Response.Cookies.Append("UserIdentifier", userGuid, new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddYears(30),
            HttpOnly = true
        });

        user = new User
        {
            CookieIdentifier = userGuid,
            Email = string.Empty
        };

        _userManager.AddUser(user);
        return user;
    }
}