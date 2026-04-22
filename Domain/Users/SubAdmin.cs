using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.project;
using Microsoft.AspNetCore.Authorization;

namespace IntegratieProject.BL.Domain.users;

[Authorize(Roles = "SubAdmin")]

public class SubAdmin : IAdmin
{
    public int Id { get; set; }

    public IEnumerable<Project> Projects { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [MaxLength(300)]
    public string IdentityUserId { get; set; }
    [Required]
    public GeneralAdmin GeneralAdmin { get; set; }
    [Required]
    public SubPlatform SubPlatform { get; set; }
}