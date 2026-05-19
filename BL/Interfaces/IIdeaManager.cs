using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.interfaces;

public interface IIdeaManager
{
    Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text, int? userId, string imageUri = null);
    Task ForceSubmitIdeaAsync(int topicId, string title, string text, int? userId, string imageUri = null);

    IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null);
    IEnumerable<Idea> GetIdeasInReviewBySubPlatform(int subPlatformId);
    IEnumerable<Idea> GetIdeasBySubPlatform(int subPlatformId, int? projectId = null);

    void ApproveIdea(int ideaId);
    void RejectIdea(int ideaId);

    Task<string> ImproveIdeaTextAsync(string title, string text, string language = "");
    Task<string> ImproveIdeaTextAsync(int ideaId);

    Task<List<string>> GenerateIdeaFollowUpQuestionsAsync(string title, string text);

    Task SubmitIdeaWithoutAiModerationAsync(int topicId, string title, string text, int? userId,
        string imageUri = null);

    Task<ToxicityResult> ModerateIdeaOnlyAsync(string title, string text);
}