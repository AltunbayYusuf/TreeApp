using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.DAL.Ef;

public class Repository : IRepository
{
    private readonly TreeDbContext _context;

    public Repository(TreeDbContext context)
    {
        _context = context;
    }
}