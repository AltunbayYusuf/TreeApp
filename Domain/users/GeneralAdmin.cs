namespace IntergratieProject.Domain.users;

public class GeneralAdmin : Admin
{
    public IEnumerable<SubAdmin> SubAdmins { get; set; }
    
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}