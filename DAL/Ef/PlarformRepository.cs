using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.DAL.Ef;

public class PlarformRepository : IPlarformRepository
{
    private readonly TreeDbContext _context;

    public PlarformRepository(TreeDbContext context)
    {
        _context = context;
    }
}