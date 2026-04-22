using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;

namespace IntergratieProject.BL.interfaces;

public interface IIdeaManager
{
    public Task<string> AskAiForIdea(string idea);
    Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text, int? userId);
    Task ForceSubmitIdeaAsync(int topicId, string title, string text, int? userId);
    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null);
    IEnumerable<Idea> GetIdeasInReviewBySubPlatform(int subPlatformId);

    void ApproveIdea(int ideaId);
    void RejectIdea(int ideaId);
}