using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.interfaces;

public interface ITopicManager
{
    public IEnumerable<Topic> GetTopicsByProject(Project project);
}