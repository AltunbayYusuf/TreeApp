using IntergratieProject.BL;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.users;
using IntergratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class SurveyController : Controller
{
    private readonly IManager _manager;

    public SurveyController(IManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public IActionResult Index(string subplatform, int projectId)
    {
        var project = _manager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);

        if (project == null)
        {
            return NotFound();
        } 
        if (project.Status != Status.Active)
        {
            return NotFound(); 
        }

        

        var user = GetOrCreateUser();

        var existingResponse = _manager.GetSurveyResponse(user.Id, projectId);
        if (existingResponse != null)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.SubPlatformSlug = subplatform;
            return View("Results", existingResponse);
        }

        ViewBag.ProjectId = projectId;
        ViewBag.SubPlatformSlug = subplatform;

        var questions = _manager.GetQuestionListByProject(project);
        return View(questions);
    }

    [HttpPost]
    public IActionResult Submit(string subplatform,List<AnswerDto> answers, int projectId)
    {
        var project = _manager.GetProjectBySubPlatformAndProjectId(subplatform, projectId);
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

        var existingResponse = _manager.GetSurveyResponse(user.Id, projectId);
        if (existingResponse != null)
        {
            return BadRequest("Deze survey is al ingevuld voor dit project.");
        }

        var answersList = new List<Answer>();

        foreach (var dto in answers)
        {
            var question = _manager.GetQuestion(dto.QuestionId);
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

        _manager.SaveSurveyResponse(user.Id, projectId, answersList);

        return Ok(new
        {
            redirectUrl = Url.Action("Index", "Survey", new { subplatform, projectId })
        });
    }

    private User GetOrCreateUser()
    {
        string userGuid = Request.Cookies["UserIdentifier"];
        User? user = null;

        if (!string.IsNullOrEmpty(userGuid))
        {
            user = _manager.GetUser(userGuid);
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
            _manager.AddUser(user);
        }

        return user;
    }
}