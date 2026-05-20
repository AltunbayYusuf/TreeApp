using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.UI.MVC.Models;

public class CreateProjectInfoViewModel : IValidatableObject
{
    public const string DefaultFontFamily = "Inter";

    public static readonly string[] AllowedFontFamilies =
    {
        DefaultFontFamily,
        "Arial",
        "Georgia",
        "Verdana",
        "Trebuchet MS",
        "Pacifico",
        "Bubblegum Sans"
    };

    public string SubplatformSlug { get; set; } = "";

    [Required(ErrorMessage = "Projectnaam is verplicht.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Introductietekst is verplicht.")]
    [MinLength(4, ErrorMessage = "Introductietekst moet minstens 4 tekens bevatten.")]
    [MaxLength(1000, ErrorMessage = "Introductietekst mag maximaal 1000 tekens bevatten.")]    
    public string Introduction { get; set; } = "";

    [Range(1, 999, ErrorMessage = "Tijdsduur moet minstens 1 minuut zijn.")]
    public int Duration { get; set; } = 10;

    public IFormFile? IntroMediaUpload { get; set; }

    public string IntroMediaUri { get; set; }

    public ProjectIntroMediaType IntroMediaType { get; set; } = ProjectIntroMediaType.Image;

    [Required(ErrorMessage = "Lettertype is verplicht.")]
    [MaxLength(50)]
    public string FontFamily { get; set; } = DefaultFontFamily;

    public ProjectType Type { get; set; } = ProjectType.VerticalScroll;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!AllowedFontFamilies.Contains(FontFamily))
        {
            yield return new ValidationResult(
                "Kies een geldig lettertype.",
                new[] { nameof(FontFamily) });
        }
    }
}