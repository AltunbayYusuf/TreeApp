using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.questions;

namespace IntegratieProject.BL.Domain.Questions;

public class ConditionalQuestion
{
    public int Id { get; set; }

    [Required]
    public Question ParentQuestion { get; set; } = null!;

    [Required]
    public Question FollowUpQuestion { get; set; } = null!;

    [Required]
    public string TriggerValue { get; set; } = string.Empty;

    public TriggerType TriggerType { get; set; }
}