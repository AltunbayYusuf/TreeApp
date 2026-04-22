using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IntegratieProject.DAL.Identity;

public class ApplicationUser : IdentityUser
{
    [MaxLength]
    public string SubPlatformSlug { get; set; }
}