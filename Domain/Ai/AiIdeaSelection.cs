using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.BL.Domain.Ai;

public class AiIdeaSelection
{
    public int Id { get; set; }

    [Required] public int ProjectId { get; set; }

    [Required] [MaxLength(50)] public string SelectionMode { get; set; } = "";

    [Required] public string ResultJson { get; set; } = "";

    [Range(0, int.MaxValue)] public int IdeaCountAtGeneration { get; set; }

    [Range(0, int.MaxValue)] public int ReactionCountAtGeneration { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}