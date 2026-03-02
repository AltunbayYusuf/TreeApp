using IntergratieProject.Domain.Ai;

namespace IntergratieProject.Domain.ideas;

public class Idea
{
    public string Title { get; set; }
    public string Text { get; set; }
    public ModerationStatus ModerationStatus { get; set; }
    public IEnumerable<Reaction> Reactions { get; set; }
    
    public AiIntegration AiIntegration { get; set; }
}