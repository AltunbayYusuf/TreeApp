using IntegratieProject.BL.Domain.users;
using IntegratieProject.DAL.interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class UserRepository : IUserRepository
{
    private readonly TreeDbContext _context;

    public UserRepository(TreeDbContext context)
    {
        _context = context;
    }

    public User ReadUser(string cookieId)
    {
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
            .Include(g => g.SubAdmins)
            .ThenInclude(s => s.SubPlatform)
            .ThenInclude(sp => sp.Projects)
            .FirstOrDefault();
    }
}