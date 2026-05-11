using IntegratieProject.BL.Domain.users;
using IntegratieProject.DAL.Identity;
using Microsoft.AspNetCore.Identity;

namespace IntegratieProject.BL.interfaces;

public interface IUserManager
{
    User GetUser(string cookieId);
    void AddUser(User user);
    void UpdateUser(User user);
    IEnumerable<SubAdmin> GetAllSubAdmins();    
    GeneralAdmin GetGeneralAdmin();
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
    Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role);
}