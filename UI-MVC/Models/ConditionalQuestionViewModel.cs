using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.Questions;

namespace IntegratieProject.UI.MVC.Models;

public class ConditionalQuestionViewModel
{
    [Required]
    public string Trigger { get; set; } = string.Empty;
    
    public TriggerType TriggerType { get; set; } = TriggerType.Contains;


    [Required]
    public string QuestionText { get; set; } 

    public bool UseAi { get; set; }
}