using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.users;

namespace IntergratieProject.BL;

public interface IManager
{
   Task<ToxicityResult> AddReaction(int ideaId, string emoji, string text, int? userId);
   Task ForceAddReactionAsync(int ideaId, string? emoji, string? text, int? userId);
   
    public Task<string> AskAiForIdea(string idea);
    //Task<(bool IsToxic, string SuggestedText, string Explanation)> ModerateTextAsync(string input);
    Task<ToxicityResult> ModerateTextAsync(string input);
    Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text);

    Task ForceSubmitIdeaAsync(int topicId, string title, string text);
   // Task<ToxicityResult> SubmitReactionAsync(int ideaId, string emoji, string text);

    public IEnumerable<Topic> GetTopicsByProject(Project project);
    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null);
    IEnumerable<Question> GetAllQuestionsBySection(int sectionId);
    IEnumerable<Question> GetAllQuestions();
    Question GetQuestion(int questionId);
    QuestionList GetQuestionListByProject(Project projectId);
    Project? GetProject(int projectId);
    User? GetUser(string cookieId);
    void AddUser(User user);

    SurveyResponse? GetSurveyResponse(int userId, int projectId);
    void SaveSurveyResponse(int userId, int projectId, List<Answer> answers);
    
    SubPlatform? GetSubPlatformBySlug(string slug);
    Project? GetProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId);
    IEnumerable<Project> GetProjectsBySubPlatform(int subPlatformId);
    
    Project? GetFirstProjectBySubPlatform(string slug);
    
    void UpdateProject(Project project);
    
    void CreateProject(Project project);
    
    IEnumerable<Idea> GetIdeasInReviewBySubPlatform(int subPlatformId);
    IEnumerable<Reaction> GetReactionsInReviewBySubPlatform(int subPlatformId);
    
    void ApproveIdea(int ideaId);
    void RejectIdea(int ideaId);
    void ApproveReaction(int reactionId);
    void RejectReaction(int reactionId);
}