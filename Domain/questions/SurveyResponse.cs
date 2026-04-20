using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.Questions;

public class SurveyResponse
{
    public int Id { get; set; }

    [Required(ErrorMessage = "SurveyResponse must belong to a projectID")]
    public int ProjectId { get; set; }

    public Project Project { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "SurveyResponse must have answers")]
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}