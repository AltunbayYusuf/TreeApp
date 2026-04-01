using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;
[Authorize(Roles = "GeneralAdmin")]
[Authorize(Roles = "SubAdmin")]
public class SubAdminController: Controller
{
 
    public IActionResult Index()
    {
     
        return View();
    }

}