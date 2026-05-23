using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.Domain.Ai;

public class AiModelConfiguration
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string FeatureKey { get; set; } = string.Empty;

    [Required] [MaxLength(50)] public string Provider { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string ModelName { get; set; } = string.Empty;

    [Range(0, double.MaxValue)] public decimal InputCostPerMillionTokens { get; set; }
    [Range(0, double.MaxValue)] public decimal OutputCostPerMillionTokens { get; set; }

    [Range(0, double.MaxValue)] public decimal ImageCostPerImage { get; set; }

    [Required] [MaxLength(10)] public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;

    public int? SubPlatformId { get; set; }
    public SubPlatform SubPlatform { get; set; }
}