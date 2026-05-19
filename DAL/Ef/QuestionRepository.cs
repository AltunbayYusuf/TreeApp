using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.DAL.interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntegratieProject.DAL.Ef;

public class QuestionRepository : IQuestionRepository
{
    private readonly TreeDbContext _context;

    public QuestionRepository(TreeDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Question> ReadAllQuestionsBySection(int sectionId)
    {
        return _context.Questions
            .Include(q => q.Answers)
            .Include(q => q.Image)
            .Where(q => q.Section.Id == sectionId)
            .ToList();
    }

    public IEnumerable<Question> ReadAllQuestions()
    {
        return _context.Questions
            .Include(q => q.Answers)
            .Include(q => q.Image).ToList();
    }

    public Question ReadQuestion(int questionId)
    {
        return _context.Questions.FirstOrDefault(q => q.Id == questionId);
    }

    public QuestionList ReadQuestionListByProject(Project project)
    {
        return _context.Projects
            .Include(p => p.QuestionList)
            .ThenInclude(ql => ql.Sections)
            .ThenInclude(s => s.Questions)
            .ThenInclude(q => q.Options)

            .Include(p => p.QuestionList)
            .ThenInclude(ql => ql.Sections)
            .ThenInclude(s => s.Questions)
            .ThenInclude(q => q.Image)

            .Include(p => p.QuestionList)
            .ThenInclude(ql => ql.Sections)
            .ThenInclude(s => s.Questions)
            .ThenInclude(q => q.ConditionalQuestions)
            .ThenInclude(cq => cq.FollowUpQuestion)

            .Where(p => p.Id == project.Id)
            .Select(p => p.QuestionList)
            .FirstOrDefault();
    }

    public void SaveQuestionList(QuestionList questionList)
    {
        _context.QuestionList.Add(questionList);
        _context.SaveChanges();
    }
}