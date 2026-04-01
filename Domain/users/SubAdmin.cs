using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.users;

public class SubAdmin : IAdmin
{
    public int Id { get; set; }

    public IEnumerable<Project> Projects { get; set; }
    
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    
    [Required]
    public GeneralAdmin GeneralAdmin { get; set; }
    [Required]
    public SubPlatform SubPlatform { get; set; }
}