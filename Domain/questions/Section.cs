using System.ComponentModel.DataAnnotations;

namespace IntergratieProject.Domain.Questions;

public class Section
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Section must have a title")]
    [MaxLength(200)]
    public string Title { get; set; }
    public int Order { get; set; }
    [Required(ErrorMessage = "Section must have a questionList")]
    public IEnumerable<Question> Questions { get; set; }
    public QuestionList QuestionList { get; set; }
}