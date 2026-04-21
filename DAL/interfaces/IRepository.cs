using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;

namespace IntergratieProject.DAL.interfaces;

public interface IRepository
{
    IEnumerable<Topic> ReadTopicsByProject(Project project);
    Topic? ReadTopicById(int topicId);
    SubPlatform? ReadSubPlatformBySlug(string slug);
}