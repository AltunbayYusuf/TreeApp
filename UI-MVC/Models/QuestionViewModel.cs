using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.questions;

namespace IntegratieProject.UI.MVC.Models;

public class QuestionViewModel
{
    [Required]
    public string Description { get; set; } = "";

    public QuestionType QuestionType { get; set; }

    public List<string> Options { get; set; } = new();

    public int? RangeMin { get; set; }
    public int? RangeMax { get; set; }
    public List<ConditionalQuestionViewModel> Conditionals { get; set; } = new();

}