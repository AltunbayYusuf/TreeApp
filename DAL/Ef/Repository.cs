using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class Repository : IRepository
{
    private readonly TreeDbContext _context;

    public Repository(TreeDbContext context)
    {
        _context = context;
    }

}