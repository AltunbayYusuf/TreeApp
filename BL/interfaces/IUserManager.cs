using IntergratieProject.Domain.users;

namespace IntergratieProject.BL.interfaces;

public interface IUserManager
{
    User? GetUser(string cookieId);
    void AddUser(User user);
    void UpdateUser(User user);
}