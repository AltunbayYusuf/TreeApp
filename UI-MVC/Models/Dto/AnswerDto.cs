using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models.Dto;

public class AnswerDto
{
    [Required(ErrorMessage = "Answer must belong to a question")]
    public int QuestionId { get; set; }
    [Required(ErrorMessage = "Answer must have a value")]
    public string Value { get; set; }
}