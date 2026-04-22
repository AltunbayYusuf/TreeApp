using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.BL.Domain.questions;

public class QuestionOption
{
    public int Id { get; set; }

    [Required(ErrorMessage = "QuestionOption must have a text")]
    [MaxLength(200)]
    public string Text { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; }
}