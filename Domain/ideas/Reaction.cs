using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.ideas;

public class Reaction
{
    public int Id { get; set; }
    public ModerationStatus ModerationStatus  { get; set; }
    public string Text { get; set; }
    public string Emoji { get; set; }
    public User User { get; set; }
    public Idea Idea  { get; set; }
}