using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Domain.users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.Api;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
[ApiController]
[Route("api/[controller]")]
public class PlatformsController : Controller
{

}