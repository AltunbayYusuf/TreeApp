using IntergratieProject.BL.interfaces;
using IntergratieProject.DAL.interfaces;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.BL;

public class SurveyManager : ISurveyManager
{
    private readonly ISurveyRepository _surveyRepository;

    public SurveyManager(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    public SurveyResponse? GetSurveyResponse(int userId, int projectId)
    {
        return _surveyRepository.ReadSurveyResponse(userId, projectId);
    }

    public void SaveSurveyResponse(int userId, int projectId, List<Answer> answers)
    {
        _surveyRepository.SaveSurveyResponse(userId, projectId, answers);
    }
}