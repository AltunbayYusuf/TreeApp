using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.users;

namespace IntegratieProject.BL.Domain.project;

public class SubPlatform
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Subplatform must have a company name")]
    [MaxLength(50)]
    public string CompanyName { get; set; }

    [Required(ErrorMessage = "Subplatform must have a slug")]
    [MaxLength(50)]
    public string Slug { get; set; }

    public Media Logo { get; set; }

    public Platform Platform { get; set; }
    public Language Language { get; set; }

    public ICollection<SubAdmin> SubAdmins { get; set; } = new List<SubAdmin>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}