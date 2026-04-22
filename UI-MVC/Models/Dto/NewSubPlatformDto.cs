using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.UI.MVC.Models.Dto;

public class NewSubPlatformDto
{
    [Required(ErrorMessage = "Subplatform must have a company name")]
    public string CompanyName { get; set; }
    public Platform Platform { get; set; }
    public Language Language { get; set; }
}