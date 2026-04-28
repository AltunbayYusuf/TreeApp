namespace IntegratieProject.BL.Ai;

public class SurveyGenerationResult
{
    public List<SurveySectionResult> Sections { get; set; } = new();

}
public class SurveySectionResult
{
    public string Title { get; set; } = "";
    public List<SurveyQuestionResult> Questions { get; set; } = new();
}

public class SurveyQuestionResult
{
    public string Title { get; set; } = "";
    
    public string Type { get; set; } = "";

    public List<string> Answers { get; set; } = new();

    public string Min { get; set; } = "";
    public string Max { get; set; } = "";

    public List<SurveyConditionalResult> Conditionals { get; set; } = new();
}

public class SurveyConditionalResult
{
    public string Trigger { get; set; } = "";
    public bool Ai { get; set; }
    public string Question { get; set; } = "";
}