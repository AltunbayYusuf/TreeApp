using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Nodig voor IFormFile

namespace IntegratieProject.UI.MVC.Models.Dto;

public class CreateSubPlatformDto
{
    public string CompanyName { get; set; }

    [Required]
    [RegularExpression(@"^[a-z][a-z0-9]*-[a-z0-9][a-z0-9-]*$",
        ErrorMessage = "Slug moet beginnen met een kleine letter...")]
    public string Slug { get; set; }

    public string AdminEmail { get; set; }
    
    public IFormFile LogoFile { get; set; } 
}