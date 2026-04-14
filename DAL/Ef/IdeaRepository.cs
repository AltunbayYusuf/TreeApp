using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.users;
using Microsoft.EntityFrameworkCore;

namespace IntergratieProject.DAL.Ef;

public class IdeaRepository : IIdeaRepository
{
    private readonly TreeDbContext _context;

    public IdeaRepository(TreeDbContext context)
    {
        _context = context;
    }

    public void AddIdea(Idea idea)
    {
        _context.Ideas.Add(idea);
        _context.SaveChanges();
    }
    
    public void AddReaction(Reaction reaction)
    {
        _context.Reactions.Add(reaction);
        _context.SaveChanges();
    }
    public IEnumerable<Topic> ReadTopicsByProject(Project project)
    {
        return _context.Topics.Where(t => t.Project == project).ToList();
    }

    public IEnumerable<Idea> ReadIdeasByProject(Project project)
    {
        return _context.Ideas.Include(i => i.Topic)
           // .Include(i => i.Reactions)
            .Include(i => i.Reactions.Where(r => r.ModerationStatus == ModerationStatus.Accepted))
            .Where(i => i.Topic.Project == project)
            .ToList();
    }

    public IEnumerable<Idea> ReadIdeasByTopic(Project project, int topicId)
    {
        return _context.Ideas.Include(i => i.Topic)
          //  .Include(i => i.Reactions)
            .Include(i => i.Reactions.Where(r => r.ModerationStatus == ModerationStatus.Accepted))
            .Where(i => i.Topic.Project == project && i.Topic.Id == topicId)
            .ToList();
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

    public Topic? ReadTopicById(int topicId)
    {
        return _context.Topics.Include(t => t.Ideas)
            .FirstOrDefault(t => t.Id == topicId);
    }

    public Idea? ReadIdeaById(int ideaId)
    {
        return _context.Ideas.Include(i => i.Topic)
            .Include(i => i.Reactions)
            .FirstOrDefault(i => i.Id == ideaId);
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
            .Where(p => p.Id == project.Id)
            .Select(p => p.QuestionList)
            .FirstOrDefault();
    }

    public Project? ReadProject(int projectId)
    {
        return _context.Projects
            .Include(p => p.SubPlatform)
            .Include(p => p.Photo)
            .Include(p => p.Logo)
            .FirstOrDefault(p => p.Id == projectId);
    }

    public User? ReadUser(string cookieId)
    {
        return _context.Users
            .Include(u => u.SurveyResponses)
                .ThenInclude(sr => sr.Answers)
                    .ThenInclude(a => a.Question)
            .FirstOrDefault(u => u.CookieIdentifier == cookieId);
    }

    public void CreateUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public SurveyResponse? ReadSurveyResponse(int userId, int projectId)
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
    
    public SubPlatform? ReadSubPlatformBySlug(string slug)
    {
        return _context.SubPlatforms
            .FirstOrDefault(sp => sp.Slug == slug);
    }

    public Project? ReadProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId)
    {
        return _context.Projects
            .Include(p => p.SubPlatform)
            .Include(p => p.Photo)
            .Include(p => p.Logo)
            .FirstOrDefault(p => p.Id == projectId && p.SubPlatform.Slug == subplatformSlug);
    }
    
    public IEnumerable<Project> ReadProjectsBySubPlatform(int subPlatformId)
    {
        return _context.Projects
            .Include(p => p.SubPlatform)
            .Include(p => p.Photo)
            .Include(p => p.Logo)
            .Where(p => p.SubPlatformId == subPlatformId)
            .ToList();
    }
}