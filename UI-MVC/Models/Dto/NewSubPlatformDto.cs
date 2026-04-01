using IntergratieProject.Domain.project;

namespace IntergratieProject.UI.MVC.Models.Dto;

public class NewSubPlatformDto
{
    public string CompanyName { get; set; }
    public Platform Platform { get; set; }
    public Language Language { get; set; }
}