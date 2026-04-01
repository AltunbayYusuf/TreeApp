using IntergratieProject.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

[Authorize(Roles = "SubAdmin")]
public class SubPlatformController : Controller
{
    private readonly IManager _manager;

    public SubPlatformController(IManager manager)
    {
        _manager = manager;
    }
    public IActionResult Index(int id)
    {
        var subplatform = _manager.GetSubPlatform(id);

        if (subplatform == null)
        {
            return NotFound();
        }
        
        return View(subplatform);
    }

}