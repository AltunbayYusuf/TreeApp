using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.BL.Domain.Ai;

public class AiOpenQuestionSummary
{
    public int Id { get; set; }

    [Required] public int ProjectId { get; set; }

    [Required] public int QuestionId { get; set; }

    [Required] public string Summary { get; set; } = "";

    [Range(0, int.MaxValue)] public int AnswerCountAtGeneration { get; set; }

    [Range(0, int.MaxValue)] public int LastAnswerIdAtGeneration { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}