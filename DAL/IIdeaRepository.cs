using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.DAL;

public interface IIdeaRepository
{
    // void addIdea(Idea idea);
    
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