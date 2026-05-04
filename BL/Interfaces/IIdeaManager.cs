using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.interfaces;

public interface IIdeaManager
{
    Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text, int? userId);
    Task ForceSubmitIdeaAsync(int topicId, string title, string text, int? userId);
    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null);
    IEnumerable<Idea> GetIdeasInReviewBySubPlatform(int subPlatformId);
    IEnumerable<Idea> GetIdeasBySubPlatform(int subPlatformId, int? projectId = null);

    void ApproveIdea(int ideaId);
    void RejectIdea(int ideaId);
    Task<string> ImproveIdeaTextAsync(string title, string text);
    Task<string> ImproveIdeaTextAsync(int ideaId);

}