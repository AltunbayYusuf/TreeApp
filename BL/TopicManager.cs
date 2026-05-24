using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class TopicManager : ITopicManager
{
    private readonly ITopicRepository _topicRepository;

    public TopicManager(ITopicRepository topicRepository)
    {
        _topicRepository = topicRepository;
    }


    public IEnumerable<Topic> GetTopicsByProject(Project project)
    {
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        return _topicRepository.ReadTopicsByProject(project);
    }

    public IEnumerable<Topic> GetTopicsBySubPlatform(int subPlatformId)
    {
        if (subPlatformId <= 0)
        {
            throw new ArgumentException("Geen subplatform gevonden", nameof(subPlatformId));
        }

        return _topicRepository.ReadTopicsBySubPlatform(subPlatformId);
    }
}