using IntergratieProject.BL;
using IntergratieProject.Domain.project;
using IntergratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubPlatformsController : ControllerBase
{
    private readonly IManager _manager;

    public SubPlatformsController(IManager manager)
    {
        _manager = manager;
    }

    // [HttpPost]
    // [Authorize]
    // public ActionResult<Project> AddProject(NewProjectDto project)
    // {
    //
    //
    //     return CreatedAtAction();
    // }

}