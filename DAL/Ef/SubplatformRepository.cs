using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.users;
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

    public void CreateSubPlatform(SubPlatform subPlatform)
    {
        _context.SubPlatforms.Add(subPlatform);
        _context.SaveChanges();
    }

    public void CreateSubAdmin(SubAdmin subAdmin)
    {
        _context.SubAdmins.Add(subAdmin);
        _context.SaveChanges();
    }

    public Platform ReadPlatform()
    {
        return _context.Platforms.First();
    }

 
}