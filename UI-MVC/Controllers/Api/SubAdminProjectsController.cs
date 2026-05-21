using System.Text.Json;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Domain.Questions;
using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.UI.MVC.Models;
using IntegratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.Api;

[ApiController]
[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
[Route("api/subadmin-projects")]
public class SubAdminProjectsController : ControllerBase
{
    private const int MaxRangeValue = 10;

    private readonly IAiSurveyGenerationService _aiSurveyGenerationService;
    private readonly IAiSummaryIdeas _aiSummaryIdeas;
    private readonly IAiIdeaSelectionService _aiIdeaSelectionService;
    private readonly ISubplatformManager _subplatformManager;

    public SubAdminProjectsController(
        IAiSurveyGenerationService aiSurveyGenerationService,
        IAiSummaryIdeas aiSummaryIdeas,
        IAiIdeaSelectionService aiIdeaSelectionService,
        ISubplatformManager subplatformManager)
    {
        _aiSurveyGenerationService = aiSurveyGenerationService;
        _aiSummaryIdeas = aiSummaryIdeas;
        _aiIdeaSelectionService = aiIdeaSelectionService;
        _subplatformManager = subplatformManager;
    }

    private const string SurveyKey = "CreateProject_Survey";

    [HttpPost("survey")]
    public IActionResult SaveSurvey([FromBody] SaveSurveyRequestDto request)
    {
        var subplatform = HttpContext.Items["subplatform"]?.ToString() ?? "";

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
                        RangeMin = int.TryParse(q.Min, out var min) ? Math.Min(min, MaxRangeValue) : null,
                        RangeMax = int.TryParse(q.Max, out var max) ? Math.Min(max, MaxRangeValue) : null,
                        Conditionals = q.Conditionals
                            .Where(c => !string.IsNullOrWhiteSpace(c.Trigger))
                            .Select(c => new ConditionalQuestionViewModel
                            {
                                Trigger = c.Trigger,
                                TriggerType = Enum.TryParse<TriggerType>(c.TriggerType, out var triggerType)
                                    ? triggerType
                                    : TriggerType.Contains,
                                QuestionText = c.Question,
                                UseAi = c.Ai
                            })
                            .ToList()
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
            redirectUrl = Url.Action("CreateIdeation", "SubAdminProjects", new { subplatform })
        });
    }

    [HttpPost("survey/ai")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSurvey([FromBody] GenerateSurveyRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Beschrijving is verplicht."
            });
        }

        var subplatform = HttpContext.Items["subplatform"]?.ToString() ?? "";
        var subplatformEntity = _subplatformManager.GetSubPlatformBySlug(subplatform);

        var survey = await _aiSurveyGenerationService.GenerateSurveyAsync(
            request.Description,
            request.QuestionAmount,
            subplatformEntity?.Id
        );

        return Ok(new
        {
            ok = true,
            survey,
            message = "Vragenlijst gegenereerd."
        });
    }

    [HttpPost("summary")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateProjectSummary(
        string subplatform,
        [FromBody] GenerateProjectSummaryDto dto)
    {
        if (dto == null || dto.ProjectId <= 0)
        {
            return BadRequest(new
            {
                ok = false,
                message = "Ongeldig project."
            });
        }

        var summary = await _aiSummaryIdeas.GenerateProjectTrendSummaryAsync(dto.ProjectId);

        return Ok(new
        {
            ok = true,
            summary
        });
    }

    [HttpPost("idea-selection")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateIdeaSelection(
        string subplatform,
        [FromBody] GenerateIdeaSelectionDto dto)
    {
        if (dto == null || dto.ProjectId <= 0)
        {
            return BadRequest(new
            {
                ok = false,
                message = "Ongeldig project."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.SelectionMode))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Selectiemodus is verplicht."
            });
        }

        var allowedModes = new[] { "different" };

        if (!allowedModes.Contains(dto.SelectionMode))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Ongeldige selectiemodus."
            });
        }

        var resultJson = await _aiIdeaSelectionService.GenerateIdeaSelectionAsync(
            dto.ProjectId,
            dto.SelectionMode
        );

        try
        {
            using var document = JsonDocument.Parse(resultJson);

            return Ok(new
            {
                ok = true,
                selection = document.RootElement.Clone()
            });
        }
        catch (JsonException)
        {
            return Ok(new
            {
                ok = false,
                message = "AI gaf geen geldige JSON terug.",
                rawResult = resultJson
            });
        }
    }
}