using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models;

public class ConditionalQuestionViewModel
{
    [Required]
    public string Trigger { get; set; } = string.Empty;

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    public bool UseAi { get; set; }
}