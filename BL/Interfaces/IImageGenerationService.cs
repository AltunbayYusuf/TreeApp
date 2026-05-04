namespace IntegratieProject.BL.Interfaces;

public interface IImageGenerationService
{
    Task<string> GenerateProjectImageAsync(string projectName);


}