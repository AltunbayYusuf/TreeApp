using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class SurveyManager : ISurveyManager
{
    private readonly ISurveyRepository _surveyRepository;

    public SurveyManager(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    public SurveyResponse GetSurveyResponse(int userId, int projectId)
    {
        return _surveyRepository.ReadSurveyResponse(userId, projectId);
    }

    public void SaveSurveyResponse(
        int userId,
        int projectId,
        List<Answer> answers,
        int durationInSeconds
    )
    {
        _surveyRepository.SaveSurveyResponse(
            userId,
            projectId,
            answers,
            durationInSeconds
        );
    }
}