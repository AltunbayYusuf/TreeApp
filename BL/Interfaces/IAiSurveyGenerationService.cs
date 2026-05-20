using IntegratieProject.BL.Ai;

namespace IntegratieProject.BL.Interfaces;

public interface IAiSurveyGenerationService
{
    Task<SurveyGenerationResult> GenerateSurveyAsync(string description, int questionAmount, int? subPlatformId = null);
}