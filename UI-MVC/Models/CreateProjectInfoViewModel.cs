using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.UI.MVC.Models;

public class CreateProjectInfoViewModel
{
    public string SubplatformSlug { get; set; } = "";

    [Required(ErrorMessage = "Projectnaam is verplicht.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Introductietekst is verplicht.")]
    [MinLength(4, ErrorMessage = "Introductietekst moet minstens 4 tekens bevatten.")]
    [MaxLength(1000, ErrorMessage = "Introductietekst mag maximaal 1000 tekens bevatten.")]    
    public string Introduction { get; set; } = "";
    
    [Range(1, 999, ErrorMessage = "Tijdsduur moet minstens 1 minuut zijn.")]
    public int Duration { get; set; } = 10;
    public IFormFile? PhotoUpload { get; set; }

    public string? PhotoUri { get; set; }
    public ProjectType Type { get; set; } = ProjectType.VerticalScroll;}