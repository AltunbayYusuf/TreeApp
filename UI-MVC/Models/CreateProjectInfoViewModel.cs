using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.project;

namespace IntergratieProject.UI.MVC.Models;

public class CreateProjectInfoViewModel
{
    public string SubplatformSlug { get; set; } = "";

    [Required(ErrorMessage = "Projectnaam is verplicht.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Introductietekst is verplicht.")]
    public string Introduction { get; set; } = "";

    public IFormFile? PhotoUpload { get; set; }

    public ProjectType Type { get; set; } = ProjectType.VerticalScroll;}