using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.questions;

namespace IntegratieProject.BL.Domain.users;

public class User
{
    public int Id { get; set; }

    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Voer een geldig e-mailadres in")]
    [MaxLength(200)]
    public string Email { get; set; }

    [Required(ErrorMessage = "User must have a cookieIdentifier")]
    [MaxLength(1000)]
    public string CookieIdentifier { get; set; } = string.Empty;

    public ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}