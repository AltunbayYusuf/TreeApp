namespace IntergratieProject.Domain.Ai;

public class AiIntegration
{
    public int Id { get; set; }
    public string ModelName { get; set; }
    public string Prompt { get; set; }
    public string Response { get; set; }
    public int MaxTokens { get; set; }
    public FeatureType Feature { get; set; }
    public DateTime CreatedAt { get; set; }
}