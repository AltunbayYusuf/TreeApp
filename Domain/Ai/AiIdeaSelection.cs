namespace IntegratieProject.BL.Domain.Ai;

public class AiIdeaSelection
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string SelectionMode { get; set; }

    public string ResultJson { get; set; }

    public int IdeaCountAtGeneration { get; set; }

    public int ReactionCountAtGeneration { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}