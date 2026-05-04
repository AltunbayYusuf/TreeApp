using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.DAL.Ef;

public class AiRepository :  IAiRepository
{
    private readonly TreeDbContext _context;

    public AiRepository(TreeDbContext context)
    {
        _context = context;
    }
    
    public AiPrompt ReadAiPromptByKey(string key)
    {
        return _context.AiPrompts.FirstOrDefault(p => p.Key == key && p.IsActive);
    }
}