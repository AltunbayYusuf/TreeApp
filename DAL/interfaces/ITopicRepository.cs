using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.DAL.interfaces;

public interface ITopicRepository
{
    IEnumerable<Topic> ReadTopicsByProject(Project project);
    Topic ReadTopicById(int topicId);
}