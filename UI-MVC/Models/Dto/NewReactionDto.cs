using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models.Dto;

public class NewReactionDto
{
    [Required(ErrorMessage = "Reaction must belong to an idea")]
    public int? IdeaId { get; set; }
    public string Emoji { get; set; }
    public string Text { get; set; }
    public bool SkipAiModeration { get; set; }
    
}