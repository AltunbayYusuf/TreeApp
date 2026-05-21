namespace IntegratieProject.BL.Domain.Statistics;

public class ProjectStatistics
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }

    public int ParticipantsCount { get; set; }
    public int IdeasCount { get; set; }
    public int ReactionsCount { get; set; }

    public List<QuestionStatistics> Questions { get; set; } = new();
}

public class QuestionStatistics
{
    public int QuestionId { get; set; }
    public string Question { get; set; }
    public string QuestionType { get; set; }

    public int TotalAnswers { get; set; }

    public List<AnswerOptionStatistics> Options { get; set; } = new();

    public double? Average { get; set; }

    public string AiSummary { get; set; }
    public DateTime? AiSummaryGeneratedAt { get; set; }
    public bool AiSummaryNeedsRefresh { get; set; }
    public List<string> OpenAnswers { get; set; } = new();
}

public class AnswerOptionStatistics
{
    public string Answer { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}