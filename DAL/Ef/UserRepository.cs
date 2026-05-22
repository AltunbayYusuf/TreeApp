using IntegratieProject.BL.Domain.users;
using IntegratieProject.DAL.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IntegratieProject.DAL.Identity;

namespace IntegratieProject.DAL.Ef;

public class UserRepository : IUserRepository
{
    private readonly TreeDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(TreeDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public User ReadUser(string cookieId)
    {
        if (string.IsNullOrEmpty(cookieId))
        {
            throw new ArgumentException("Geen user gevonen", nameof(cookieId));
        }
        return _context.Users
            .Include(u => u.SurveyResponses)
            .ThenInclude(sr => sr.Answers)
            .ThenInclude(a => a.Question)
            .FirstOrDefault(u => u.CookieIdentifier == cookieId);
    }

    public void CreateUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void UpdateUser(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public IEnumerable<SubAdmin> ReadAllSubAdminsWithPlatformsAndProjects()
    {
        return _context.SubAdmins
            .Include(sa => sa.SubPlatform)
            .ThenInclude(p => p.Projects)
            .ToList();
    }

    public GeneralAdmin GetGeneralAdmin()
    {
        return _context.GeneralAdmins
            .Include(g => g.Platform)
            .ThenInclude(p => p.SubPlatforms)
            .ThenInclude(sp => sp.Projects)
            .Include(g => g.SubAdmins)
            .ThenInclude(sa => sa.SubPlatform)
            .ThenInclude(sp => sp.Projects)
            .ThenInclude(p => p.SurveyResponses)
            .FirstOrDefault();
    }

    public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }
}