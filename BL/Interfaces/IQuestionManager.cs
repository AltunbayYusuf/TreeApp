using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.interfaces;

public interface IQuestionManager
{
    IEnumerable<Question> GetAllQuestionsBySection(int sectionId);
    IEnumerable<Question> GetAllQuestions();
    Question GetQuestion(int questionId);
    QuestionList GetQuestionListByProject(Project projectId);
    void SaveQuestionList(QuestionList questionList);
}