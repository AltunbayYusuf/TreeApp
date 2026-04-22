using IntegratieProject.BL.Domain.users;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class UserManager : IUserManager
{
    private readonly IUserRepository _userRepository;
    private readonly IManager _manager;

    public UserManager(IUserRepository userRepository, IManager manager)
    {
        _userRepository = userRepository;
        _manager = manager;
    }

    public User GetUser(string cookieId)
    {
        return _userRepository.ReadUser(cookieId);
    }

    public void AddUser(User user)
    {
        _manager.ValidateEntity(user);
        _userRepository.CreateUser(user);
    }

    public void UpdateUser(User user)
    {
        _manager.ValidateEntity(user);
        _userRepository.UpdateUser(user);
    }
}