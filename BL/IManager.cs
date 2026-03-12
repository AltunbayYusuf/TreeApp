using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.users;

namespace IntergratieProject.BL;

public interface IManager
{
   // void AddReaction(int ideaId, string emoji, string text);
   Task<ToxicityResult> AddReaction(int ideaId, string emoji, string text);
    public Task<string> AskAiForIdea(string idea);
    //Task<(bool IsToxic, string SuggestedText, string Explanation)> ModerateTextAsync(string input);
    Task<ToxicityResult> ModerateTextAsync(string input);

   // Task<(bool Saved, bool IsToxic, string SuggestedText, string Explanation)> SubmitIdeaAsync(int topicId,
  //      string title, string text);
  Task<ToxicityResult> SubmitIdeaAsync(int topicId, string title, string text);

    Task ForceSubmitIdeaAsync(int topicId, string title, string text);
   // Task<ToxicityResult> SubmitReactionAsync(int ideaId, string emoji, string text);

    public IEnumerable<Topic> GetTopicsByProject(Project project);
    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null);
    IEnumerable<Question> GetAllQuestionsBySection(int sectionId);
    IEnumerable<Question> GetAllQuestions();
    Question GetQuestion(int questionId);
    QuestionList GetQuestionListByProject(Project projectId);
    Project GetProject(int projectId);
    User GetUser(string cookieId);
    void AddUser(User user);
    void SaveAnswers(int userId, List<Answer> answers);
}

   