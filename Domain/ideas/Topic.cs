
namespace IntergratieProject.Domain.ideas;

public class Topic
{
    private int Id { get; set; }
    private String Thema { get; set; }
    
    private IEnumerable<Idea> Ideas { get; set; }
}