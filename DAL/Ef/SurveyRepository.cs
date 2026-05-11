using IntegratieProject.BL.Domain.questions;
using IntegratieProject.DAL.interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class SurveyRepository : ISurveyRepository
{
    private readonly TreeDbContext _context;

    public SurveyRepository(TreeDbContext context)
    {
        _context = context;
    }

    public SurveyResponse ReadSurveyResponse(int userId, int projectId)
    {
        return _context.SurveyResponses
            .Include(sr => sr.Answers)
            .ThenInclude(a => a.Question)
            .Include(sr => sr.Project)
            .ThenInclude(p => p.SubPlatform)
            .FirstOrDefault(sr => sr.UserId == userId && sr.ProjectId == projectId);
    }

    public void SaveSurveyResponse(int userId, int projectId, List<Answer> answers)
    {
        var surveyResponse = new SurveyResponse
        {
            UserId = userId,
            ProjectId = projectId,
            SubmittedAt = DateTime.UtcNow,
            Answers = answers
        };

        _context.SurveyResponses.Add(surveyResponse);
        _context.SaveChanges();
    }
    
    public IEnumerable<SurveyResponse> ReadSurveyResponsesByProjectId(int projectId)
    {
        return _context.SurveyResponses
            .Include(sr => sr.Answers)
            .ThenInclude(a => a.Question)
            .Include(sr => sr.Project)
            .Where(sr => sr.ProjectId == projectId)
            .OrderByDescending(sr => sr.SubmittedAt)
            .ToList();
    }
}