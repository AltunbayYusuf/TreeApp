using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models;

public class SubmitIdeaViewModel
{
    [Required(ErrorMessage = "Idea must belong to a topic")]
    
    public int TopicId { get; set; }

    public string Title { get; set; }
    public string Text { get; set; }
    public string FollowUpAnswers { get; set; }
    public bool SkipAiModeration { get; set; }

    public bool ContactOptIn { get; set; }
    public string Email { get; set; }
    
    public string SubplatformSlug { get; set; }

    public IFormFile ImageUpload { get; set; }
}