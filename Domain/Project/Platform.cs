using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.users;

namespace IntegratieProject.BL.Domain.project;

public class Platform
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Platform must have a company name")]
    [MaxLength(50)]
    public string CompanyName { get; set; }

    [Required] 
    public GeneralAdmin GeneralAdmin { get; set; }

    public IEnumerable<SubPlatform> SubPlatforms { get; set; }
}