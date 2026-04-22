using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class TopicManager
{
    private readonly ITopicRepository _topicRepository;
    
    public TopicManager(ITopicRepository topicRepository)
    {
        _topicRepository = topicRepository;
    }

    
    public IEnumerable<Topic> GetTopicsByProject(Project project)
    {
        return _topicRepository.ReadTopicsByProject(project);
    }
}