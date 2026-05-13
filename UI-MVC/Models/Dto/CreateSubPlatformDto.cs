using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models.Dto;

public class CreateSubPlatformDto
{
    public string CompanyName { get; set; }

    [Required]
    [RegularExpression(@"^[a-z][a-z0-9]*-[a-z0-9][a-z0-9-]*$",
        ErrorMessage = "Slug moet beginnen met een kleine letter, alleen kleine letters/cijfers/koppeltekens bevatten, en minstens één koppelteken hebben (bv. bedrijf-naam).")]
    public string Slug { get; set; }

    public string AdminEmail { get; set; }
}