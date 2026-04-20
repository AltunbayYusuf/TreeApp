namespace IntergratieProject.UI.MVC.Models;

public class SubmitIdeaViewModel
{
    public int TopicId { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public bool ContactOptIn { get; set; }
    public string Email { get; set; }
}