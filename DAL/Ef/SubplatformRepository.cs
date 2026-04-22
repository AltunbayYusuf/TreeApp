using IntegratieProject.BL.Domain.project;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.DAL.Ef;

public class SubplatformRepository : ISubplatformRepository
{
    private readonly TreeDbContext _context;

    public SubplatformRepository(TreeDbContext context)
    {
        _context = context;
    }
    
    public SubPlatform ReadSubPlatformBySlug(string slug)
    {
        return _context.SubPlatforms
            .FirstOrDefault(sp => sp.Slug == slug);
    }
}