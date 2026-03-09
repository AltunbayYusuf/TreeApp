using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.ideas;

public class Idea
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public User? User { get; set; }
    public Topic Topic { get; set; }
    public ModerationStatus ModerationStatus { get; set; }
    public IEnumerable<Reaction> Reactions { get; set; }

    public AiIntegration AiIntegration { get; set; }
}