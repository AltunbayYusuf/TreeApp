namespace IntergratieProject.UI.MVC.Models.Dto;

public class ReactionResultDto
{
    public bool Ok { get; set; }
    public bool Saved { get; set; }
    public bool IsToxic { get; set; }
    public bool AiUnavailable { get; set; }
    public string Message { get; set; }
    public string Warning { get; set; }
    public string Explanation { get; set; }
    public string SuggestedText { get; set; }
}