using IntergratieProject.BL;
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
    public IActionResult Index(int projectId = 1)  //default om te teste dat het getoont wordt, later verwijderen
    {
        var project = _manager.GetProject(projectId);

        if (project == null)
        {
            return NotFound();
        }

        var questions= _manager.GetQuestionListByProject(project);
        return View(questions);
    }
}