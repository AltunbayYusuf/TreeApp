using IntergratieProject.Domain.Questions;

namespace IntergratieProject.DAL.interfaces;

public interface ISurveyRepository
{
    SurveyResponse? ReadSurveyResponse(int userId, int projectId);
    void SaveSurveyResponse(int userId, int projectId, List<Answer> answers);
}