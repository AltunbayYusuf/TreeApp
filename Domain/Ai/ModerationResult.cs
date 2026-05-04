namespace IntegratieProject.BL.Domain.Ai;

public class ModerationResult
{
    public bool IsAccepted { get; set; }
    public bool NeedsUserRevision { get; set; }
    public bool NeedsManualReview { get; set; }

    public bool IsToxic { get; set; }
    public bool NeedsMoreDetail { get; set; }
    public bool AiUnavailable { get; set; }

    public string Explanation { get; set; } = "";
    public string SuggestedText { get; set; } = "";
}