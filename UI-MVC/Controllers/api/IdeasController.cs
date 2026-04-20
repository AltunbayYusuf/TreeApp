using IntergratieProject.BL;
using IntergratieProject.Domain.users;
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
            var user = SaveContactPreference(vm);
            var result = await _manager.SubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text, user.Id);

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
        // catch (Exception)
        // {
        //     await _manager.ForceSubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text);
        //
        //     return Ok(new
        //     {
        //         ok = true,
        //         saved = true,
        //         isToxic = false,
        //         aiUnavailable = true,
        //         message = "Je idee werd opgeslagen en doorgestuurd voor moderatie omdat de AI-controle tijdelijk niet beschikbaar was."
        //     });
        // }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                ok = false,
                saved = false,
                isToxic = false,
                aiUnavailable = true,
                message = ex.Message,
                details = ex.InnerException?.Message
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
            var user = SaveContactPreference(vm);
            await _manager.ForceSubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text, user.Id);

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

    private User SaveContactPreference(SubmitIdeaViewModel vm)
    {
        var user = GetOrCreateUser();
        user.Email = vm.ContactOptIn && !string.IsNullOrWhiteSpace(vm.Email) ? vm.Email.Trim() : string.Empty;
        _manager.UpdateUser(user);
        return user;
    }

    private User GetOrCreateUser()
    {
        var userGuid = Request.Cookies["UserIdentifier"];
        User? user = null;

        if (!string.IsNullOrEmpty(userGuid))
        {
            user = _manager.GetUser(userGuid);
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

        _manager.AddUser(user);
        return user;
    }
}