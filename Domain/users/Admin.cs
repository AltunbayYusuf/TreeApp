namespace IntergratieProject.Domain.users;

public interface Admin
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}