
namespace IntergratieProject.Domain.ideas;

public class Topic
{
    private int Id { get; set; }
    private string Thema { get; set; }
    
    private IEnumerable<Idea> Ideas { get; set; }
}