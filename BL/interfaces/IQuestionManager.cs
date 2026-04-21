using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.BL.interfaces;

public interface IQuestionManager
{
    IEnumerable<Question> GetAllQuestionsBySection(int sectionId);
    IEnumerable<Question> GetAllQuestions();
    Question GetQuestion(int questionId);
    QuestionList GetQuestionListByProject(Project projectId);
    void SaveQuestionList(QuestionList questionList);
}