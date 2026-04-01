using IntergratieProject.DAL.Ef;
using IntergratieProject.Domain.users;

namespace IntergratieProject.BL;

public class AdminService
{
    private readonly TreeDbContext _context;

    public GeneralAdmin GetByIdentityId(string id)
    {
        return _context.GeneralAdmins
            .FirstOrDefault(a => a.IdentityUserId == id);
    }
}