using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.socialeMedia;

namespace IntergratieProject.Domain.project;

public class Project
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

    [Required(ErrorMessage = "Project must have topics")]
    public IEnumerable<Topic> Topics { get; set; } = new List<Topic>();

    public SocialMediaPost SocialMediaPost { get; set; }
    public AiIntegration AiIntegration { get; set; }

    [Required(ErrorMessage = "Project must have questions")]
    public QuestionList QuestionList { get; set; }

    public ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
}