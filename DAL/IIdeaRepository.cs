using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.DAL;

public interface IIdeaRepository
{
     void AddIdea(Idea idea);
    void AddReaction(int ideaId, string? emoji, string? text);
    
    IEnumerable<Topic> ReadTopicsByProject(Project project);
    IEnumerable<Idea> ReadIdeasByProject(Project project);
    IEnumerable<Idea> ReadIdeasByTopic(Project project, int topicId);
    IEnumerable<Question> ReadAllQuestionsBySection(int sectionId);
    IEnumerable<Question> ReadAllQuestions();
    
    Topic? ReadTopicById(int topicId);
    Idea? ReadIdeaById(int ideaId);
    QuestionList ReadQuestionListByProject(Project projectId);
    
    Project ReadProject(int projectId);
    
}