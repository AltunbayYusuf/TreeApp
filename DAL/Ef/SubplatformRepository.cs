using IntegratieProject.BL.Domain.project;
using IntegratieProject.DAL.interfaces;
using Microsoft.EntityFrameworkCore;

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

    public SubPlatform ReadSubPlatform(int subPlatformId)
    {
        return _context.SubPlatforms
            .Include(sp => sp.Projects)
            .ThenInclude(p => p.SurveyResponses)
            .Include(sp => sp.SubAdmins)
            .FirstOrDefault(sp => sp.Id == subPlatformId);
    }
}