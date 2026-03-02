using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.users;

public class SubAdmin : Admin
{
    public IEnumerable<Project> Projects { get; set; }
    
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public GeneralAdmin GeneralAdmin { get; set; }
    public SubPlatform SubPlatform { get; set; }
}