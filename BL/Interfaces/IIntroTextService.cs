namespace IntegratieProject.BL.Interfaces;

public interface IIntroTextService
{
    Task<string> GenerateIntroAsync(string projectName);
}