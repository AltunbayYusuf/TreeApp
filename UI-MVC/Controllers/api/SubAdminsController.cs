using IntergratieProject.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.api;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminsController : ControllerBase
{
    private readonly IManager _manager;

    public SubAdminsController(IManager manager)
    {
        _manager = manager;
    }
}