using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.DAL.Interfaces;

public interface IRepository
{
    IEnumerable<Topic> ReadTopicsByProject(Project project);
    Topic? ReadTopicById(int topicId);
    SubPlatform? ReadSubPlatformBySlug(string slug);
}