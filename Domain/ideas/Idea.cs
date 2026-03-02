namespace IntergratieProject.Domain.ideas;

public class Idea
{
    public string Titel { get; set; }
    public string Tekst { get; set; }
    public ModerationSatatus ModerationSatatus { get; set; }
    public IEnumerable<Reaction> Reactions { get; set; }
    
    // public AiInteraction AiInteraction { get; set; }
    
}