using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.project;
using Microsoft.AspNetCore.Authorization;

namespace IntergratieProject.Domain.users;

[Authorize(Roles = "SubAdmin")]
public class SubAdmin : IAdmin
{
    public int Id { get; set; }

    public IEnumerable<Project> Projects { get; set; }
    [Required]
    public string Name { get; set; }
    
    public string IdentityUserId { get; set; }
    [Required]
    public GeneralAdmin GeneralAdmin { get; set; }
    [Required]
    public SubPlatform SubPlatform { get; set; }
}