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
        if (userId <= 0) 
            throw new ArgumentException("UserId moet groter zijn dan 0.", nameof(userId));
            
        if (projectId <= 0) 
            throw new ArgumentException("ProjectId moet groter zijn dan 0.", nameof(projectId));

        return _surveyRepository.ReadSurveyResponse(userId, projectId);
    }

    public void SaveSurveyResponse(
        int userId,
        int projectId,
        List<Answer> answers,
        int durationInSeconds
    )
    {
        if (userId <= 0) 
            throw new ArgumentException("Geen user gevonden", nameof(userId));
            
        if (projectId <= 0) 
            throw new ArgumentException("Geen project gevonden", nameof(projectId));

        if (answers == null) 
            throw new ArgumentNullException(nameof(answers), "De lijst met antwoorden mag niet leeg zijn.");
            
        if (!answers.Any()) 
            throw new ArgumentException("Er moet minstens één antwoord gegeven zijn.", nameof(answers));

        if (durationInSeconds < 0) 
            throw new ArgumentOutOfRangeException(nameof(durationInSeconds), "De invulduur kan niet negatief zijn.");

        _surveyRepository.SaveSurveyResponse(
            userId,
            projectId,
            answers,
            durationInSeconds
        );
    }
}