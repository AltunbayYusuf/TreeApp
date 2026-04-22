using IntegratieProject.BL.Domain.questions;

namespace IntegratieProject.DAL.interfaces;

public interface ISurveyRepository
{
    SurveyResponse ReadSurveyResponse(int userId, int projectId);
    void SaveSurveyResponse(int userId, int projectId, List<Answer> answers);
}