namespace IntegratieProject.BL.Interfaces;

public interface IImageGenerationService
{
    Task<string> GenerateProjectImageAsync(string title, string description);
}