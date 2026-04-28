namespace IntegratieProject.BL.Interfaces;

public interface IAiProvider
{
    Task<string> GenerateAsync(string prompt);
    

}