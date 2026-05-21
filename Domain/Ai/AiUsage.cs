using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.Domain.Ai;

public class AiUsage
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Feature { get; set; } 
    public string Model { get; set; } 

    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }

    public decimal EstimatedCost { get; set; }
    public string Currency { get; set; } = "USD";

    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int? SubPlatformId { get; set; }
    public SubPlatform SubPlatform { get; set; }
}