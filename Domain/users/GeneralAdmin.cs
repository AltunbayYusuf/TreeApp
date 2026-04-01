using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.users;

public class GeneralAdmin : IAdmin
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    
    
    public IEnumerable<SubAdmin> SubAdmins { get; set; }
    
    public Platform Platform { get; set; }
}