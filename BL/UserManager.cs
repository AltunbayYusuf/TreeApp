using IntegratieProject.BL.Domain.users;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.Identity;
using IntegratieProject.DAL.interfaces;
using Microsoft.AspNetCore.Identity;

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
        if (string.IsNullOrWhiteSpace(cookieId))
        {
            throw new ArgumentException("Geen user gevonden", nameof(cookieId));
        }

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

    public IEnumerable<SubAdmin> GetAllSubAdmins()
    {
        return _userRepository.ReadAllSubAdminsWithPlatformsAndProjects();
    }

    public GeneralAdmin GetGeneralAdmin()
    {
        return _userRepository.GetGeneralAdmin();
    }

    public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password mag niet leeg zijn.", nameof(password));
        }

        return await _userRepository.CreateUserAsync(user, password);
    }

    public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role mag niet leeg zijn.", nameof(role));
        }

        return await _userRepository.AddUserToRoleAsync(user, role);
    }
}