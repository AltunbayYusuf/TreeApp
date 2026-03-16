using IntergratieProject.BL;
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
    public IActionResult Index(int projectId) 
    {
        var project = _manager.GetProject(projectId);

        if (project == null)
        {
            return NotFound();
        }

        var user = GetOrCreateUser();
    
        if (user.Answers != null && user.Answers.Any())
        {
            ViewBag.ProjectId = projectId;
            return View("Results", user); 
        }
        
        ViewBag.ProjectId = projectId;
        var questions = _manager.GetQuestionListByProject(project);
        return View(questions);
    }

    [HttpPost]
    public IActionResult Submit(List<AnswerDto> answers, int projectId)
    {
        var project = _manager.GetProject(projectId);
        if (project == null)
        {
            return NotFound();
        }

        if (answers == null || !answers.Any())
        {
            return BadRequest("Geen antwoorden ontvangen");
        }

        var user = GetOrCreateUser();
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
                Question = question,
                Text = dto.Value ?? string.Empty,
                User = user
            };
            answersList.Add(newAnswer);
        }

        _manager.SaveAnswers(user.Id, answersList);

        return Ok(new { redirectUrl = Url.Action("Index", new { projectId }) });
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