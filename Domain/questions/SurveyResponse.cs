using IntergratieProject.Domain.project;
using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.Questions;

public class SurveyResponse
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}