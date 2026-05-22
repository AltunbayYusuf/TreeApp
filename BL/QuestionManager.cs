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
        if (sectionId <= 0)
            throw new ArgumentException("SectionId moet groter zijn dan 0.", nameof(sectionId));

        return _questionRepository.ReadAllQuestionsBySection(sectionId);
    }

    public IEnumerable<Question> GetAllQuestions()
    {
        return _questionRepository.ReadAllQuestions();
    }

    public Question GetQuestion(int questionId)
    {
        if (questionId <= 0)
            throw new ArgumentException("QuestionId moet groter zijn dan 0.", nameof(questionId));

        var question = _questionRepository.ReadQuestion(questionId);

        if (question == null)
            throw new KeyNotFoundException($"Vraag met ID {questionId} werd niet gevonden.");

        return question;
    }

    public QuestionList GetQuestionListByProject(Project project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project), "Project mag niet null zijn.");

        if (project.Id <= 0)
            throw new ArgumentException("Het opgegeven project heeft geen geldig ID.");

        return _questionRepository.ReadQuestionListByProject(project);
    }

    public void SaveQuestionList(QuestionList questionList)
    {
        if (questionList == null)
            throw new ArgumentNullException(nameof(questionList), "De vragenlijst mag niet null zijn.");
        
        _questionRepository.SaveQuestionList(questionList);
    }
}