using System.ComponentModel.DataAnnotations;

namespace IntergratieProject.Domain.Questions;

public class Answer
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Answer must have a text")]
    [MaxLength(200)]
    public string Text { get; set; }
    [Required(ErrorMessage = "Answer must must belong to a question")]
    public int QuestionId { get; set; }
    public Question Question { get; set; }
    public int SurveyResponseId { get; set; }
    public SurveyResponse SurveyResponse { get; set; }
}