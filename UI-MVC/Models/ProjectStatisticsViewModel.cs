namespace IntegratieProject.UI.MVC.Models;

public class ProjectStatisticsViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }

    public int ParticipantsCount { get; set; }
    public int IdeasCount { get; set; }
    public int ReactionsCount { get; set; }

    public List<QuestionStatisticsViewModel> Questions { get; set; } = new();
}

public class QuestionStatisticsViewModel
{
    public int QuestionId { get; set; }
    public string Question { get; set; }
    public string QuestionType { get; set; }

    public int TotalAnswers { get; set; }

    public List<AnswerOptionStatisticsViewModel> Options { get; set; } = new();

    public double? Average { get; set; }
}

public class AnswerOptionStatisticsViewModel
{
    public string Answer { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}