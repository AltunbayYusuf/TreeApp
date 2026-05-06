namespace IntegratieProject.BL.Domain.Ai;

public class AiOpenQuestionSummary
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public int QuestionId { get; set; }

    public string Summary { get; set; } = string.Empty;

    public int AnswerCountAtGeneration { get; set; }
    public int LastAnswerIdAtGeneration { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}