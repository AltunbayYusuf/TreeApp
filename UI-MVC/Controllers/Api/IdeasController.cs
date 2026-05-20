using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.users;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models;
using IntegratieProject.UI.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IntegratieProject.UI.MVC.Controllers.api;

[ApiController]
[Route("api/[controller]")]
public class IdeasController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;
    private readonly IUserManager _userManager;
    private readonly ISubplatformManager _subplatformManager;
    private readonly IGoogleCloudStorageService _googleCloudStorageService;
    private readonly ILogger<IdeasController> _logger;

    public IdeasController(IIdeaManager ideaManager, IUserManager userManager, ISubplatformManager subplatformManager, IGoogleCloudStorageService googleCloudStorageService, ILogger<IdeasController> logger)
    {
        _ideaManager = ideaManager;
        _userManager = userManager;
        _subplatformManager = subplatformManager;
        _googleCloudStorageService = googleCloudStorageService;
        _logger = logger;
    }


    [HttpGet]
    [Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
    public IActionResult GetIdeas([FromQuery] int subplatformId, [FromQuery] int? projectId = null, [FromQuery] string search = null)
    {
        if (subplatformId <= 0)
            return BadRequest(new { message = "Ongeldig subplatformId." });

        var ideas = _ideaManager.GetIdeasBySubPlatform(subplatformId, projectId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            ideas = ideas.Where(i => 
                (!string.IsNullOrWhiteSpace(i.Title) && i.Title.ToLower().Contains(searchLower)) ||
                (!string.IsNullOrWhiteSpace(i.Text) && i.Text.ToLower().Contains(searchLower)) ||
                (i.Topic != null && !string.IsNullOrWhiteSpace(i.Topic.Theme) && i.Topic.Theme.ToLower().Contains(searchLower)) ||
                (i.Topic != null && i.Topic.Project != null && !string.IsNullOrWhiteSpace(i.Topic.Project.Name) && i.Topic.Project.Name.ToLower().Contains(searchLower))
            );
        }

        var result = ideas.Select(i => new
        {
            id = i.Id,
            title = string.IsNullOrWhiteSpace(i.Title) ? "Zonder titel" : i.Title,
            text = i.Text,
            status = i.ModerationStatus.ToString(),
            topic = i.Topic?.Theme ?? "-",
            project = i.Topic?.Project?.Name ?? "-",
            projectId = i.Topic?.Project?.Id,
            userEmail = string.IsNullOrWhiteSpace(i.User?.Email) ? null : i.User.Email,
            reactions = (i.Reactions ?? Enumerable.Empty<Reaction>())
                .Select(r => new
                {
                    id = r.Id,
                    text = r.Text ?? "",
                    emoji = r.Emoji ?? "",
                    status = r.ModerationStatus.ToString()
                })
        });

        return Ok(result);
    }

    [HttpPost]
    [EnableRateLimiting("ai-limit")]
    public async Task<IActionResult> SubmitIdea([FromForm] SubmitIdeaViewModel vm)
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

        var contactValidationError = ValidateContactEmail(vm);
        if (contactValidationError != null)
        {
            return BadRequest(new
            {
                ok = false,
                message = contactValidationError
            });
        }

        string imageUri = null;

        if (vm.ImageUpload != null && vm.ImageUpload.Length > 0)
        {
            imageUri = await _googleCloudStorageService.UploadProjectMediaAsync(
                vm.ImageUpload,
                vm.SubplatformSlug ?? "unknown"
            );
        }

        var user = SaveContactPreference(vm);

        var result = await _ideaManager.SubmitIdeaAsync(
            vm.TopicId,
            vm.Title,
            vm.Text,
            user.Id,
            imageUri
        );

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
                suggestedTitle = result.SuggestedTitle,
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
    public async Task<IActionResult> ForceSubmitIdea([FromForm] SubmitIdeaViewModel vm)
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

        var contactValidationError = ValidateContactEmail(vm);
        if (contactValidationError != null)
        {
            return BadRequest(new
            {
                ok = false,
                message = contactValidationError
            });
        }

        string imageUri = null;

        if (vm.ImageUpload != null && vm.ImageUpload.Length > 0)
        {
            imageUri = await _googleCloudStorageService.UploadProjectMediaAsync(
                vm.ImageUpload,
                vm.SubplatformSlug ?? "unknown"
            );
        }

        var user = SaveContactPreference(vm);

        await _ideaManager.ForceSubmitIdeaAsync(
            vm.TopicId,
            vm.Title,
            vm.Text,
            user.Id,
            imageUri
        );

        return Ok(new
        {
            ok = true,
            saved = true,
            isToxic = false,
            aiUnavailable = false,
            message = "Idee doorgestuurd voor moderatie."
        });
    }

    [HttpDelete("{ideaId:int}")]
    [Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
    public IActionResult DeleteIdea(int ideaId)
    {
        if (ideaId <= 0)
        {
            return BadRequest(new { message = "Ongeldig idee." });
        }

        _ideaManager.RejectIdea(ideaId);
        return Ok(new { ok = true });
    }


    private User SaveContactPreference(SubmitIdeaViewModel vm)
    {
        var user = GetOrCreateUser();
        user.Email = vm.ContactOptIn && !string.IsNullOrWhiteSpace(vm.Email) ? vm.Email.Trim() : string.Empty;
        _userManager.UpdateUser(user);
        return user;
    }

    private static string ValidateContactEmail(SubmitIdeaViewModel vm)
    {
        if (!vm.ContactOptIn)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(vm.Email))
        {
            return "Geef je e-mailadres in als je gecontacteerd wil worden.";
        }

        return new EmailAddressAttribute().IsValid(vm.Email.Trim())
            ? null
            : "Geef een geldig e-mailadres in.";
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
    
    [HttpPost("improve")]
    public async Task<IActionResult> ImproveIdea([FromBody] ImproveIdeaViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Text))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Tekst is leeg."
            });
        }

        try
        {
            var improvedJson = await _ideaManager.ImproveIdeaTextAsync(vm.Title ?? "", vm.Text, vm.Language);

            using var document = JsonDocument.Parse(improvedJson);
            var root = document.RootElement;

            var improvedTitle = root.GetProperty("title").GetString();
            var improvedText = root.GetProperty("text").GetString();

            return Ok(new
            {
                ok = true,
                improvedTitle,
                improvedText
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ImproveIdea mislukt");
            return StatusCode(503, new
            {
                ok = false,
                message = "AI tijdelijk niet beschikbaar."
            });
        }
    }
}
