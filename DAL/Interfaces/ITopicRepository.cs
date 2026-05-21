using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.DAL.interfaces;

public interface ITopicRepository
{
    IEnumerable<Topic> ReadTopicsByProject(Project project);
    IEnumerable<Topic> ReadTopicsBySubPlatform(int subPlatformId);
    Topic ReadTopicById(int topicId);
}
