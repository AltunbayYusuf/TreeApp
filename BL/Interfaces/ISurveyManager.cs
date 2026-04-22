using IntegratieProject.BL.Domain.questions;

namespace IntegratieProject.BL.interfaces;

public interface ISurveyManager
{
    SurveyResponse GetSurveyResponse(int userId, int projectId);
    void SaveSurveyResponse(int userId, int projectId, List<Answer> answers);
}