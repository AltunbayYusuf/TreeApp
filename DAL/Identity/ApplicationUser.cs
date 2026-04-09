using Microsoft.AspNetCore.Identity;

namespace IntergratieProject.DAL.Identity;

public class ApplicationUser : IdentityUser
{
    public string SubPlatformSlug { get; set; }
}