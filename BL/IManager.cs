using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.BL;

public interface  IManager
{
    public Task<string> AskAiForIdea(string idea);
    Task<(bool IsToxic, string SuggestedText, string Explanation)> ModerateTextAsync(string input);
    public IEnumerable<Topic> GetTopicsByProject(Project project);
    public IEnumerable<Idea> GetIdeasByProject(Project project, int? topicId = null);
    IEnumerable<Question> GetAllQuestionsBySection(int sectionId);
    IEnumerable<Question> GetAllQuestions();
}