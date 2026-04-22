using IntegratieProject.BL.Domain.users;

namespace IntegratieProject.DAL.interfaces;

public interface IUserRepository
{
    User ReadUser(string cookieId);
    void CreateUser(User user);
    void UpdateUser(User user);

}