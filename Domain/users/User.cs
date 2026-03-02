using IntergratieProject.Domain.ideas;

namespace IntergratieProject.Domain.users;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }

    public IEnumerable<Idea> Ideas { get; set; }
    public IEnumerable<Reaction> Reactions { get; set; }
}   