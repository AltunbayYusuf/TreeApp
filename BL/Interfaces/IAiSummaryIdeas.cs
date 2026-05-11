namespace IntegratieProject.BL.Interfaces;

public interface IAiSummaryIdeas
{
    Task<string> GenerateProjectTrendSummaryAsync(int projectId);
    Task<string> GenerateOpenQuestionSummaryAsync(int projectId, int questionId);


}