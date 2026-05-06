using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models;
using IntegratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
public class PlatformController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISubplatformManager _subplatformManager;

    public PlatformController(ILogger<HomeController> logger, ISubplatformManager subplatformManager)
    {
        _logger = logger;
        _subplatformManager = subplatformManager;
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var subPlatform = _subplatformManager.GetSubPlatform(id);

        if (subPlatform == null) return NotFound();

        return View(subPlatform);
    }

    [HttpPost]
    public IActionResult CreateSubPlatform([FromBody] CreateSubPlatformDto dto)
    {
        _subplatformManager.CreateSubPlatform(
            dto.CompanyName,
            dto.Slug,
            dto.ContactEmail,
            dto.AdminEmail
        );

        return Ok();
    }
}