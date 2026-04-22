using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

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