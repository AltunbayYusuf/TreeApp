using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.users;

namespace IntergratieProject.DAL;

public interface IIdeaRepository
{
    void AddIdea(Idea idea);
    void AddReaction(Reaction reaction);

    IEnumerable<Topic> ReadTopicsByProject(Project project);
    IEnumerable<Idea> ReadIdeasByProject(Project project);
    IEnumerable<Idea> ReadIdeasByTopic(Project project, int topicId);
    IEnumerable<Question> ReadAllQuestionsBySection(int sectionId);
    IEnumerable<Question> ReadAllQuestions();
    Question ReadQuestion(int questionId);
    Topic? ReadTopicById(int topicId);
    Idea? ReadIdeaById(int ideaId);
    QuestionList ReadQuestionListByProject(Project project);

    Project? ReadProject(int projectId);
    User? ReadUser(string cookieId);
    void CreateUser(User user);

    SurveyResponse? ReadSurveyResponse(int userId, int projectId);
    void SaveSurveyResponse(int userId, int projectId, List<Answer> answers);
    
    SubPlatform? ReadSubPlatformBySlug(string slug);
    Project? ReadProjectBySubPlatformAndProjectId(string subplatformSlug, int projectId);
    IEnumerable<Project> ReadProjectsBySubPlatform(int subPlatformId);
    
    IEnumerable<Idea> ReadIdeasInReviewBySubPlatform(int subPlatformId);
    IEnumerable<Reaction> ReadReactionsInReviewBySubPlatform(int subPlatformId);
}