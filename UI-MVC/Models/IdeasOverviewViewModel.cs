using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;

namespace IntergratieProject.UI.MVC.Models;

public class IdeasOverviewViewModel
{
    public Project Project { get; set; }
    public int? SelectedTopicId { get; set; }
    public IEnumerable<Topic> Topics { get; set; } = new List<Topic>();
    public IEnumerable<Idea> Ideas { get; set; } = new List<Idea>();
    
    // Voor nieuw idee
    public int NewIdeaTopicId { get; set; }
    public string NewIdeaTitle { get; set; } = "";
    public string NewIdeaText { get; set; } = "";
}