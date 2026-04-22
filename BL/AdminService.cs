using IntegratieProject.BL.Domain.users;
using IntegratieProject.DAL.Ef;

namespace IntegratieProject.BL
{
    public class AdminService
    {
        private readonly TreeDbContext _context;
        
        public AdminService(TreeDbContext context)
        {
            _context = context;
        }

        public GeneralAdmin GetByIdentityId(string id)
        {
            return _context.GeneralAdmins
                .FirstOrDefault(a => a.IdentityUserId == id);
        }
    }
}