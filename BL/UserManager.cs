using IntergratieProject.BL.interfaces;
using IntergratieProject.DAL.interfaces;
using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.users;

namespace IntergratieProject.BL;

public class UserManager : IUserManager
{
    private readonly IUserRepository _userRepository;
    private readonly IManager _manager;

    public UserManager(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public User? GetUser(string cookieId)
    {
        return _userRepository.ReadUser(cookieId);
    }

    public void AddUser(User user)
    {
        _manager.ValidateEntety(user);
        _userRepository.CreateUser(user);
    }

    public void UpdateUser(User user)
    {
        _manager.ValidateEntety(user);
        _userRepository.UpdateUser(user);
    }
}