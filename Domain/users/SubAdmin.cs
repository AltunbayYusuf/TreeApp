using IntergratieProject.Domain.project;
using Microsoft.AspNetCore.Authorization;

namespace IntergratieProject.Domain.users;
[Authorize(Roles = "SubAdmin")]
public class SubAdmin : Admin
{
    public int Id { get; set; }

    public IEnumerable<Project> Projects { get; set; }

    public string Name { get; set; }

    public string IdentityUserId { get; set; }
    public ApplicationUser IdentityUser { get; set; }
    public GeneralAdmin GeneralAdmin { get; set; }
    public SubPlatform SubPlatform { get; set; }
}