using IntergratieProject.Domain.Questions;

namespace IntergratieProject.BL.interfaces;

public interface ISurveyManager
{
    SurveyResponse? GetSurveyResponse(int userId, int projectId);
    void SaveSurveyResponse(int userId, int projectId, List<Answer> answers);
}