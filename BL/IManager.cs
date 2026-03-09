using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.BL;

public interface IManager
{
    void AddReaction(int ideaId, string emoji, string text);
    public Task<string> AskAiForIdea(string idea);
    Task<(bool IsToxic, string SuggestedText, string Explanation)> ModerateTextAsync(string input);

    Task<(bool Saved, bool IsToxic, string SuggestedText, string Explanation)> SubmitIdeaAsync(int topicId,
        string title, string text);

    Task ForceSubmitIdeaAsync(int topicId, string title, string text);

    public IEnumerable<Topic> GetTopicsByProject(Project project);
    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null);
    IEnumerable<Question> GetAllQuestionsBySection(int sectionId);
    IEnumerable<Question> GetAllQuestions();
    QuestionList GetQuestionListByProject(Project projectId);
    Project GetProject(int projectId);
}

   