using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.UI.MVC.Models;

public class IdeasOverviewViewModel
{
    [Required(ErrorMessage = "IdeasOverview must belong to a project")]
    public Project Project { get; set; }
    public int? CurrentUserId { get; set; }
    public int? SelectedTopicId { get; set; }
    public IEnumerable<Topic> Topics { get; set; } = new List<Topic>();
    public IEnumerable<Idea> Ideas { get; set; } = new List<Idea>();
    public IEnumerable<string> ReactionEmojis { get; set; } = new List<string>();
    
    // Voor nieuw idee
    public int NewIdeaTopicId { get; set; }
    public string NewIdeaTitle { get; set; } = "";
    public string NewIdeaText { get; set; } = "";
}