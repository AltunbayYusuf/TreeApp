using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.project;
using Microsoft.AspNetCore.Authorization;

namespace IntergratieProject.Domain.users;
[Authorize(Roles = "GeneralAdmin")]
public class GeneralAdmin : IAdmin
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    
    public string IdentityUserId { get; set; }    
    public IEnumerable<SubAdmin> SubAdmins { get; set; }
    public Platform Platform { get; set; }
}