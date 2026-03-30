using System.Security.Claims;
using IntergratieProject.DAL.Ef;
using IntergratieProject.Domain.users;
using Microsoft.AspNetCore.Identity;

namespace IntergratieProject.BL;

public class AdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TreeDbContext _context;

    public AdminService(UserManager<ApplicationUser> userManager, TreeDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public GeneralAdmin GetByIdentityId(string id)
    {
        return _context.GeneralAdmins
            .FirstOrDefault(a => a.IdentityUserId == id);
    }

    // public GeneralAdmin GetCurrentAdmin(ClaimsPrincipal user)
    // {
    //     var id = _userManager.GetUserId(user);
    //
    //     return _context.GeneralAdmins
    //         .FirstOrDefault(a => a.IdentityUserId == id);
    // }
}