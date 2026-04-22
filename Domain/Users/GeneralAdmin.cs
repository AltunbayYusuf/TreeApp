using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.project;
using Microsoft.AspNetCore.Authorization;

namespace IntegratieProject.BL.Domain.users;

[Authorize(Roles = "GeneralAdmin")]
public class GeneralAdmin : IAdmin
{
    public int Id { get; set; }

    [Required(ErrorMessage = "GeneralAdmin must have a name")]
    [MaxLength(50)]
    public string Name { get; set; }

    public string IdentityUserId { get; set; }
    public IEnumerable<SubAdmin> SubAdmins { get; set; }

    [Required(ErrorMessage = "GeneralAdmin must belong to a platform")]

    public Platform Platform { get; set; }
}