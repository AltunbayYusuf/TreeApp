using IntergratieProject.DAL.Ef;
using IntergratieProject.Domain.users;

namespace IntergratieProject.UI.MVC.Services;

public class CurrentUserService
{
    private readonly TreeDbContext _context;
    private readonly IHttpContextAccessor _http;

    public User GetOrCreateUser()
    {
        var cookie = _http.HttpContext.Request.Cookies["user_id"];

        if (cookie == null)
        {
            var id = Guid.NewGuid().ToString();

            _http.HttpContext.Response.Cookies.Append("user_id", id);

            var user = new User { CookieIdentifier = id };
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        return _context.Users.FirstOrDefault(u => u.CookieIdentifier == cookie);
    }
}