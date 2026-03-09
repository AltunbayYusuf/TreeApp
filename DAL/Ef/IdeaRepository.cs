using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
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
    
    public void AddReaction(int ideaId, string emoji, string text)
    {
        var idea = _context.Ideas.FirstOrDefault(i => i.Id == ideaId);

        if (idea == null)
        {
            throw new Exception("Idea niet gevonden");
        }

        var reaction = new Reaction
        {
            Idea = idea,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? null : emoji,
            Text = string.IsNullOrWhiteSpace(text) ? null : text,
            ModerationStatus = ModerationStatus.InReview
        };

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
            .Include(i => i.Reactions)
            .Where(i => i.Topic.Project == project)
            .ToList();
    }

    public IEnumerable<Idea> ReadIdeasByTopic(Project project, int topicId)
    {
        return _context.Ideas.Include(i => i.Topic)
            .Include(i => i.Reactions)
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
        return _context.Projects.Include(p => p.QuestionList)
            .ThenInclude(q => q.Sections)
            .ThenInclude(s => s.Questions)
            .Where(p => p.Id == project.Id)
            .Select(p => p.QuestionList)
            .FirstOrDefault();
    }

    public Project ReadProject(int projectId)
    {
        return _context.Projects.FirstOrDefault(p => p.Id == projectId);
    }
}