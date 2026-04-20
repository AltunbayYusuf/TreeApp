using System.ComponentModel.DataAnnotations;

namespace IntergratieProject.UI.MVC.Models;

public class SubmitIdeaViewModel
{
    [Required(ErrorMessage = "Idea must belong to a topic")]
    
    public int TopicId { get; set; }

    public string Title { get; set; }
    public string Text { get; set; }
}