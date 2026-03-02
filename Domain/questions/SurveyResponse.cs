using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.Questions;

public class SurveyResponse
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public User User { get; set; }
    public List<Answer> Answers { get; set; }
}