using IntegratieProject.BL.Domain.users;

namespace IntegratieProject.BL.interfaces;

public interface IUserManager
{
    User GetUser(string cookieId);
    void AddUser(User user);
    void UpdateUser(User user);
}