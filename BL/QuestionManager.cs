using IntergratieProject.BL.interfaces;
using IntergratieProject.DAL.interfaces;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.BL;

public class QuestionManager : IQuestionManager
{
    private readonly IQuestionRepository _questionRepository;

    public QuestionManager(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public IEnumerable<Question> GetAllQuestionsBySection(int sectionId)
    {
        return _questionRepository.ReadAllQuestionsBySection(sectionId);
    }

    public IEnumerable<Question> GetAllQuestions()
    {
        return _questionRepository.ReadAllQuestions();
    }

    public Question GetQuestion(int questionId)
    {
        return _questionRepository.ReadQuestion(questionId);
    }

    public QuestionList GetQuestionListByProject(Project project)
    {
        return _questionRepository.ReadQuestionListByProject(project);
    }

    public void SaveQuestionList(QuestionList questionList)
    {
        _questionRepository.SaveQuestionList(questionList);
    }
}