using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectManager _projectManager;

    public ProjectsController(IProjectManager projectManager)
    {
        _projectManager = projectManager;
    }

    [HttpGet("{id}")]
    public ActionResult<ProjectDto> GetProject(int id)
    {
        var project = _projectManager.GetProject(id);

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