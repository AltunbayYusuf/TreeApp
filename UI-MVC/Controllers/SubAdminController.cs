using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;
[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminController: Controller
{
    public IActionResult Index()
    {
     
        return View();
    }
}