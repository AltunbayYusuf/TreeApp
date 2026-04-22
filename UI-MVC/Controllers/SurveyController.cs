using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Domain.users;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

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
    

    [HttpGet]
    public IActionResult Index(string subplatform, int projectId)
    {
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
        return View(questions);
    }

    [HttpGet]
    public IActionResult Chat(string subplatform, int projectId)
    {
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
        return View(questions);
    }

    [HttpPost]
    public IActionResult Submit(string subplatform,List<AnswerDto> answers, int projectId)
    {
        var project = _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);
        if (project == null)
        {
            return NotFound();
        }
        if (project.Status != Status.Active)
        {
            return NotFound(); 
        }
        if (answers == null || !answers.Any())
        {
            return BadRequest("Geen antwoorden ontvangen");
        }

        var user = GetOrCreateUser();

        var existingResponse = _surveyManager.GetSurveyResponse(user.Id, projectId);
        if (existingResponse != null)
        {
            return BadRequest("Deze survey is al ingevuld voor dit project.");
        }

        var answersList = new List<Answer>();

        foreach (var dto in answers)
        {
            var question = _questionManager.GetQuestion(dto.QuestionId);
            if (question == null)
            {
                return BadRequest("Ongeldige vraag.");
            }

            var newAnswer = new Answer
            {
                QuestionId = question.Id,
                Text = dto.Value ?? string.Empty
            };

            answersList.Add(newAnswer);
        }

        _surveyManager.SaveSurveyResponse(user.Id, projectId, answersList);

        return Ok(new
        {
            redirectUrl = Url.Action("Index", "Survey", new { subplatform, projectId })
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
}