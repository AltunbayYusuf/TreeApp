namespace IntegratieProject.BL.Interfaces;

public interface IAiProvider
{
    Task<string> GenerateAsync(string prompt);
    Task<byte[]> GenerateImageAsync(string prompt);
    

}