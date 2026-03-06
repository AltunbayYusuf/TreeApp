using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;

namespace IntergratieProject.Models;

public class IdeasOverviewViewModel
{
    public Project Project { get; set; }
    public int? SelectedTopicId { get; set; }
    public IEnumerable<Topic> Topics { get; set; } = new List<Topic>();
    public IEnumerable<Idea> Ideas { get; set; } = new List<Idea>();
}