using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.project;

namespace IntergratieProject.UI.MVC.Models.Dto;

public class NewSubPlatformDto
{
    [Required(ErrorMessage = "Subplatform must have a company name")]
    public string CompanyName { get; set; }
    public Platform Platform { get; set; }
    public Language Language { get; set; }
}