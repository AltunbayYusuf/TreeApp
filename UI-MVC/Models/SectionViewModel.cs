using System.ComponentModel.DataAnnotations;

namespace IntergratieProject.UI.MVC.Models;

public class SectionViewModel
{
    [Required(ErrorMessage = "Section must have a title")]
    public string Title { get; set; } = "";

    public int Order { get; set; }

    public List<QuestionViewModel> Questions { get; set; } = new();
}