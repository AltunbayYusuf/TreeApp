using IntergratieProject.BL;
using IntergratieProject.Domain.project;
using IntergratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
public class PlatformsController : ControllerBase
{
    private readonly IManager _manager;

    public PlatformsController(IManager manager)
    {
        _manager = manager;
    }
    
    // [HttpPost]
    // [Authorize]
    // public ActionResult<SubPlatform> AddProject(NewSubPlatformDto subPlatform)
    // {
    //     
    //     return CreatedAtAction();
    // }
}