using IntergratieProject.DAL.interfaces;
using IntergratieProject.Domain.users;
using Microsoft.EntityFrameworkCore;

namespace IntergratieProject.DAL.Ef;

public class UserRepository : IUserRepository
{
    private readonly TreeDbContext _context;

    public UserRepository(TreeDbContext context)
    {
        _context = context;
    }

    public User? ReadUser(string cookieId)
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
}