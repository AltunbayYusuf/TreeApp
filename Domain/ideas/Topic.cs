
using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.ideas;

public class Topic
{
    public int Id { get; set; }
    public string Theme { get; set; }
    public Project Project  { get; set; }
    public IEnumerable<Idea> Ideas { get; set; }
}