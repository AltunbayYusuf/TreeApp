using IntergratieProject.Domain.users;

namespace IntergratieProject.DAL.interfaces;

public interface IUserRepository
{
    User? ReadUser(string cookieId);
    void CreateUser(User user);
    void UpdateUser(User user);

}