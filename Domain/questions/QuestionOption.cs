using System.ComponentModel.DataAnnotations;

namespace IntergratieProject.Domain.Questions;

public class QuestionOption
{
    public int Id { get; set; }

    [Required(ErrorMessage = "QuestionOption must have a text")]
    [MaxLength(200)]
    public string Text { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; }
}