namespace IntergratieProject.Domain.Ai;

public class ToxicityResult
{
    public bool IsToxic { get; set; }
    
    public bool AiUnavailable { get; set; }
    public string SuggestedText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}