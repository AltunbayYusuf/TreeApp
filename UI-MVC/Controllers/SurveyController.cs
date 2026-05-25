using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Domain.users;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace IntegratieProject.UI.MVC.Controllers;

public class SurveyController : Controller
{
    private readonly IProjectManager _projectManager;
    private readonly IQuestionManager _questionManager;
    private readonly IUserManager _userManager;
    private readonly ISurveyManager _surveyManager;


    public SurveyController(IProjectManager projectManager,IQuestionManager questionManager,IUserManager userManager,ISurveyManager surveyManager)
    {
        _projectManager = projectManager;
        _questionManager = questionManager;
        _userManager = userManager;
        _surveyManager = surveyManager;
    }
    

    private string Subplatform => HttpContext.Items["subplatform"]?.ToString() ?? "";

    [HttpGet]
    public IActionResult Index(int projectId)
    {
        var subplatform = Subplatform;
        var project = _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);

        if (project == null)
        {
            return NotFound();
        } 
        if (project.Status != Status.Active)
        {
            return NotFound(); 
        }


        var user = GetOrCreateUser();

        var existingResponse = _surveyManager.GetSurveyResponse(user.Id, projectId);
        if (existingResponse != null)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.SubPlatformSlug = subplatform;
            return View("Results", existingResponse);
        }

        ViewBag.ProjectId = projectId;
        ViewBag.SubPlatformSlug = subplatform;

        var questions = _questionManager.GetQuestionListByProject(project);
        
        if (questions == null)
        {
            return NotFound();
        }
        return View(questions);
    }

    [HttpGet]
    public IActionResult Chat(int projectId)
    {
        var subplatform = Subplatform;
        var project = _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);

        if (project == null)
        {
            return NotFound();
        }
        if (project.Status != Status.Active)
        {
            return NotFound();
        }

        var user = GetOrCreateUser();

        var existingResponse = _surveyManager.GetSurveyResponse(user.Id, projectId);
        if (existingResponse != null)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.SubPlatformSlug = subplatform;
            return View("Results", existingResponse);
        }

        ViewBag.ProjectId = projectId;
        ViewBag.SubPlatformSlug = subplatform;

        var questions = _questionManager.GetQuestionListByProject(project);
        
        if (questions == null)
            return NotFound("Dit project heeft geen bevraging.");

        if (questions.Sections == null)
            questions.Sections = new List<Section>();

        ViewBag.SurveyQuestionsJson = ConvertQuestionsToJson(questions);
        
        return View(questions);
    }

    [HttpPost]
    public IActionResult Submit([FromBody] SubmitSurveyDto request)
    {
        var subplatform = Subplatform;
        var project = _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, request.ProjectId);
        if (project == null)
        {
            return NotFound();
        }
        if (project.Status != Status.Active)
        {
            return NotFound(); 
        }
        if (request.Answers == null || !request.Answers.Any())
        {
            return BadRequest("Geen antwoorden ontvangen");
        }

        var user = GetOrCreateUser();

        var existingResponse = _surveyManager.GetSurveyResponse(user.Id, request.ProjectId);
        if (existingResponse != null)
        {
            return BadRequest("Deze survey is al ingevuld voor dit project.");
        }

        var answersList = new List<Answer>();

        foreach (var dto in request.Answers)
        {
            var question = _questionManager.GetQuestion(dto.QuestionId);

            if (question == null)
                return BadRequest("Ongeldige vraag.");

            var newAnswer = new Answer
            {
                QuestionId = question.Id,
                Text = dto.Value ?? string.Empty
            };

            answersList.Add(newAnswer);
        }

        _surveyManager.SaveSurveyResponse(
            user.Id,
            request.ProjectId,
            answersList,
            request.DurationInSeconds
        );

        return Ok(new
        {
            redirectUrl = Url.Action("Index", "Survey", new { subplatform = Subplatform, request.ProjectId })
        });
    }

    private User GetOrCreateUser()
    {
        string userGuid = Request.Cookies["UserIdentifier"];
        User user = null;

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

            user = new User { CookieIdentifier = userGuid };
            _userManager.AddUser(user);
        }

        return user;
    }

    private static string ConvertQuestionsToJson(QuestionList questionList)
    {
        var questions = questionList.Sections?
            .Where(section => section != null)
            .SelectMany(section => section.Questions ?? Enumerable.Empty<Question>())
            .ToList() ?? new List<Question>();

        return JsonSerializer.Serialize(questions.Select(question => new
        {
            id = question.Id,
            description = question.Description,
            questionType = question.QuestionType.ToString(),
            options = (question.Options ?? Enumerable.Empty<QuestionOption>()).Select(option => new { text = option.Text }),
            rangeMin = question.RangeMin,
            rangeMax = question.RangeMax,
            rangeMinLabel = question.RangeMinLabel,
            rangeMaxLabel = question.RangeMaxLabel
        }));
    }
}