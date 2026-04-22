using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.DAL.interfaces;

public interface IIdeaRepository
{
    void AddIdea(Idea idea);
    IEnumerable<Idea> ReadIdeasByProject(Project project);
    IEnumerable<Idea> ReadIdeasByTopic(Project project, int topicId);
    Idea ReadIdeaById(int ideaId);
    IEnumerable<Idea> ReadIdeasInReviewBySubPlatform(int subPlatformId);
    void UpdateIdea(Idea idea);
    void DeleteIdea(int ideaId);
}