namespace IntegratieProject.UI.MVC.Models.Dto;

public class SaveSurveyRequestDto
{
    public List<SurveySectionDto> Sections { get; set; } = new();
}

public class SurveySectionDto
{
    public string Title { get; set; } = "";
    public List<SurveyQuestionDto> Questions { get; set; } = new();
}

public class SurveyQuestionDto
{
    public string Title { get; set; } = "";
    public string Type { get; set; } = "";
    public List<string> Answers { get; set; } = new();
    public string Min { get; set; } = "";
    public string Max { get; set; } = "";

    public List<ConditionalQuestionDto> Conditionals { get; set; } = new();
}

public class ConditionalQuestionDto
{
    public string Trigger { get; set; } = "";
    public bool Ai { get; set; }
    public string Question { get; set; } = "";
}