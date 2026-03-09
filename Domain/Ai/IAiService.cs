namespace IntergratieProject.Domain.Ai;

public interface IAiService
{
    Task<string> GenerateAsync(string prompt, FeatureType feature);

}