using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;

namespace IntergratieProject.DAL;

public interface IIdeaRepository
{
   // void addIdea(Idea idea);
    
    IEnumerable<Topic> ReadTopicsByProject(Project project);
    IEnumerable<Idea> ReadIdeasByProject(Project project);
    IEnumerable<Idea> ReadIdeasByTopic(Project project, int topicId);
    Topic? ReadTopicById(int topicId);
    Idea? ReadIdeaById(int ideaId);
    
    
    
}