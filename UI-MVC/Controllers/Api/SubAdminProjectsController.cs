using System.Text.Json;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.UI.MVC.Models;
using IntegratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.Api;

[ApiController]
[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
[Route("{subplatform}/api/subadmin-projects")]
public class SubAdminProjectsController : ControllerBase
{
    private const string SurveyKey = "CreateProject_Survey";

    [HttpPost("survey")]
    public IActionResult SaveSurvey(string subplatform, [FromBody] SaveSurveyRequestDto request)
    {
        var vm = new CreateProjecSurveyViewModel
        {
            SubplatformSlug = subplatform,
            Sections = request.Sections.Select((s, index) => new SectionViewModel
            {
                Title = string.IsNullOrWhiteSpace(s.Title) ? $"Sectie {index + 1}" : s.Title,
                Order = index + 1,
                Questions = s.Questions
                    .Where(q => !string.IsNullOrWhiteSpace(q.Title))
                    .Select(q => new QuestionViewModel
                    {
                        Description = q.Title,
                        QuestionType = q.Type switch
                        {
                            "single" => QuestionType.SingleChoice,
                            "multiple" => QuestionType.MultipleChoice,
                            "range" => QuestionType.Range,
                            _ => QuestionType.OpenQuestion
                        },
                        Options = q.Answers?
                            .Where(a => !string.IsNullOrWhiteSpace(a))
                            .ToList() ?? new List<string>(),
                        RangeMin = int.TryParse(q.Min, out var min) ? min : null,
                        RangeMax = int.TryParse(q.Max, out var max) ? max : null
                    })
                    .ToList()
            }).ToList()
        };

        if (!vm.Sections.Any() || vm.Sections.All(s => !s.Questions.Any()))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Je moet minstens 1 sectie met minstens 1 vraag invullen."
            });
        }

        HttpContext.Session.SetString(SurveyKey, JsonSerializer.Serialize(vm));

        return Ok(new
        {
            ok = true,
            redirectUrl = $"/{subplatform}/SubAdminProjects/CreateIdeation"
        });
    }
}