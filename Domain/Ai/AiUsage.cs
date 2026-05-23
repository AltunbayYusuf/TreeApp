using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.Domain.Ai;

public class AiUsage
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required] [MaxLength(100)] public string Feature { get; set; }

    [Required] [MaxLength(100)] public string Model { get; set; }

    [Range(0, int.MaxValue)] public int InputTokens { get; set; }

    [Range(0, int.MaxValue)] public int OutputTokens { get; set; }

    [Range(0, int.MaxValue)] public int TotalTokens { get; set; }

    [Range(0, double.MaxValue)] public decimal EstimatedCost { get; set; }

    [Required] [MaxLength(10)] public string Currency { get; set; } = "USD";

    [MaxLength(1000)] public string ErrorMessage { get; set; }
    public bool Success { get; set; }
    public int? SubPlatformId { get; set; }
    public SubPlatform SubPlatform { get; set; }
}