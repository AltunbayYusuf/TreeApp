using IntergratieProject.BL;
using IntergratieProject.Domain.project;
using IntergratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IManager _manager;

    public ProjectsController(IManager manager)
    {
        _manager = manager;
    }

    [HttpGet("{id}")]
    public ActionResult<ProjectDto> GetProject(int id)
    {
        var project = _manager.GetProject(id);

        if (project == null)
        {
            return NotFound();
        }
        if (project.Status != Status.Active)
        {
            return NotFound(); 
        }
        var dto = new ProjectDto
        {
            Id = project.Id,
            Introduction = project.Introduction,
            Duration = project.Duration,
            SubPlatform = project.SubPlatform?.CompanyName ?? "",
            Logo = project.Logo?.Uri ?? "",
            Photo = project.Photo?.Uri ?? ""
        };

        return Ok(dto);
    }
}