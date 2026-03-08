using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.Domain.users;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string CookieIdentifier { get; set; } = string.Empty; 
    public IEnumerable<Answer> Answers { get; set; }
    public IEnumerable<Idea> Ideas { get; set; }
    public IEnumerable<Reaction> Reactions { get; set; }
}   