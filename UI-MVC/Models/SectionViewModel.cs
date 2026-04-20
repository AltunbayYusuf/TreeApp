namespace IntergratieProject.UI.MVC.Models;

public class SectionViewModel
{
    public string Title { get; set; } = "";
    public int Order { get; set; }

    public List<QuestionViewModel> Questions { get; set; } = new();
}