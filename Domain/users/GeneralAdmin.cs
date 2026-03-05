using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.users;

public class GeneralAdmin : Admin
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    public IEnumerable<SubAdmin> SubAdmins { get; set; }
    public Platform Platform { get; set; }
}