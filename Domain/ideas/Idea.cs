using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.ideas;

public class Idea
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Idea must have a title")]
    [MaxLength(200)]
    public string Title { get; set; }

    [Required(ErrorMessage = "Idea must have a message text")]
    [MaxLength(200)]

    public string Text { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    [Required] public Topic Topic { get; set; }
    public ModerationStatus ModerationStatus { get; set; }
    public IEnumerable<Reaction> Reactions { get; set; }

    public AiIntegration AiIntegration { get; set; }
}