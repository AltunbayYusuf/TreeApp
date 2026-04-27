using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Domain.socialeMedia;

namespace IntegratieProject.BL.Domain.project;

public class Project: IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project must have a name")]
    [MaxLength(50)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Project must have an introduction")]
    [MaxLength(200)]
    public string Introduction { get; set; }

    [Required(ErrorMessage = "Project must have a status")]
    public Status Status { get; set; }
    [MaxLength(400)]
    public string Prompt { get; set; }
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }

    public ProjectType Type { get; set; }
    public Media Photo { get; set; }
    public Media Logo { get; set; }

    public int SubPlatformId { get; set; }
    public SubPlatform SubPlatform { get; set; }
    public bool HasBeenActive { get; set; }

    public IEnumerable<Topic> Topics { get; set; } = new List<Topic>();
    
    public SocialMediaPost SocialMediaPost { get; set; }
    public AiIntegration AiIntegration { get; set; }

    [Required(ErrorMessage = "Project must have questions")]
    public QuestionList QuestionList { get; set; }

    public ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Topics == null || !Topics.Any())
        {
            yield return new ValidationResult(
                "Project must have at least one topic", new[] { nameof(Topics) });
        }
    }
}
