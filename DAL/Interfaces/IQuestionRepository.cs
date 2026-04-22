using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;

namespace IntegratieProject.DAL.interfaces;

public interface IQuestionRepository
{
    IEnumerable<Question> ReadAllQuestionsBySection(int sectionId);
    IEnumerable<Question> ReadAllQuestions();
    Question ReadQuestion(int questionId);
    QuestionList ReadQuestionListByProject(Project project);
    void SaveQuestionList(QuestionList questionList);
}