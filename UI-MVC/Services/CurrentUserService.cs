using IntegratieProject.BL.Domain.users;
using IntegratieProject.DAL.Ef;

namespace IntegratieProject.UI.MVC.Services;

public class CurrentUserService
{
    private readonly TreeDbContext _context;
    private readonly IHttpContextAccessor _http;
    
    public CurrentUserService(TreeDbContext context, IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
    }

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