using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.UI.MVC.Models;

public class QuestionViewModel
{
    [Required]
    public string Description { get; set; } = "";

    public QuestionType QuestionType { get; set; }

    public List<string> Options { get; set; } = new();

    public int? RangeMin { get; set; }
    public int? RangeMax { get; set; }
}