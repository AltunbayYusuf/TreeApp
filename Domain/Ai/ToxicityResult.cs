using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.BL.Domain.Ai;

public class ToxicityResult
{
    [Required]
    public bool IsToxic { get; set; }
    
    public bool AiUnavailable { get; set; }
    [Required]
    public string SuggestedText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string SuggestedTitle { get; set; } = string.Empty;
    
}