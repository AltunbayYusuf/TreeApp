using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.Domain.Ai;

public class AiModelConfiguration
{
    public int Id { get; set; }

    public string FeatureKey { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;

    public decimal InputCostPerMillionTokens { get; set; }
    public decimal OutputCostPerMillionTokens { get; set; }
    public decimal ImageCostPerImage { get; set; }

    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;

    public int? SubPlatformId { get; set; }
    public SubPlatform SubPlatform { get; set; }
}