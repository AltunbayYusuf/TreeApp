namespace IntergratieProject.Domain.Questions;

public class SurveyResponse
{
    public int SurveyResponseId { get; set; }
    public int projectId { get; set; }
    public DateTime SubmittedAt { get; set; }
    //public User User { get; set; }
    public List<Answer> Answers { get; set; }
}