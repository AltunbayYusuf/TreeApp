namespace IntegratieProject.BL.Interfaces;

public interface IAiIdeaSelectionService
{
    Task<string> GenerateIdeaSelectionAsync(int projectId, string selectionMode);

}