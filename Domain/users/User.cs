using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.Domain.users;

public class User
{
    public int Id { get; set; }

    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Voer een geldig e-mailadres in")]
    [MaxLength(200)]
    public string Email { get; set; }

    [Required(ErrorMessage = "User must have a cookieIdentifier")]
    public string CookieIdentifier { get; set; } = string.Empty;

    public ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}