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
    
    public AiOpenQuestionSummary? ReadOpenQuestionSummary(int projectId, int questionId)
    {
        return _context.AiOpenQuestionSummaries
            .FirstOrDefault(s => s.ProjectId == projectId && s.QuestionId == questionId);
    }

    public void SaveOpenQuestionSummary(
        int projectId,
        int questionId,
        string summary,
        int answerCount,
        int lastAnswerId)
    {
        var existing = ReadOpenQuestionSummary(projectId, questionId);

        if (existing == null)
        {
            existing = new AiOpenQuestionSummary
            {
                ProjectId = projectId,
                QuestionId = questionId
            };

            _context.AiOpenQuestionSummaries.Add(existing);
        }

        existing.Summary = summary;
        existing.AnswerCountAtGeneration = answerCount;
        existing.LastAnswerIdAtGeneration = lastAnswerId;
        existing.GeneratedAt = DateTime.UtcNow;

        _context.SaveChanges();
    }
    
    public IList<AiPrompt> ReadAllAiPrompts()
    {
        return _context.AiPrompts
            .OrderBy(p => p.Name)
            .ToList();
    }

    public AiPrompt ReadAiPromptById(int id)
    {
        return _context.AiPrompts.FirstOrDefault(p => p.Id == id);
    }

    public void UpdateAiPrompt(AiPrompt prompt)
    {
        _context.AiPrompts.Update(prompt);
        _context.SaveChanges();
    }
}