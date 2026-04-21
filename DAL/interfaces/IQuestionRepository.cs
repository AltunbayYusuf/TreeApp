using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.DAL.interfaces;

public interface IQuestionRepository
{
    IEnumerable<Question> ReadAllQuestionsBySection(int sectionId);
    IEnumerable<Question> ReadAllQuestions();
    Question ReadQuestion(int questionId);
    QuestionList ReadQuestionListByProject(Project project);
    void SaveQuestionList(QuestionList questionList);
}