namespace IntegratieProject.BL.Domain.Ai;

public class AiModelOptions
{
    public List<string> TextModels { get; set; } = new();
    public List<string> ImageModels { get; set; } = new();
}