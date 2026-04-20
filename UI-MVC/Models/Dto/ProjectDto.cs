using System.ComponentModel.DataAnnotations;

namespace IntergratieProject.UI.MVC.Models.Dto;

public class ProjectDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project must have an introduction")]
    public string Introduction { get; set; }

    public int Duration { get; set; }
    public string Photo { get; set; }
    public string Logo { get; set; }

    [Required(ErrorMessage = "Project must belong to a subplatform")]
    public string SubPlatform { get; set; }
}