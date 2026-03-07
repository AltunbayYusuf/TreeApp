using IntergratieProject.BL;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class Survey : Controller
{
    private readonly IManager _manager;

    public Survey(IManager manager)
    {
        this._manager = manager;
    }
    
      
    [HttpGet]
    public IActionResult Index()
    {
        var questions= _manager.GetAllQuestions();
        return View(questions);
    } 
    
    
    
}