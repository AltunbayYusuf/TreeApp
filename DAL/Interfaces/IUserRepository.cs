using IntegratieProject.BL.Domain.users;
using IntegratieProject.DAL.Identity;
using Microsoft.AspNetCore.Identity;

namespace IntegratieProject.DAL.interfaces;

public interface IUserRepository
{
    User ReadUser(string cookieId);
    void CreateUser(User user);
    void UpdateUser(User user);
    IEnumerable<SubAdmin> ReadAllSubAdminsWithPlatformsAndProjects();
    GeneralAdmin GetGeneralAdmin();
    Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
    Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role);

}