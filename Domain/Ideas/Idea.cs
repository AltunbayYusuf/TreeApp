using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.users;

namespace IntegratieProject.BL.Domain.ideas;

public class Idea
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Idea must have a title")]
    [MaxLength(2000)]
    public string Title { get; set; }

    [Required(ErrorMessage = "Idea must have a message text")]
    [MaxLength(2000)]

    public string Text { get; set; }
    public int? UserId { get; set; }
    public User User { get; set; }
    [Required] public Topic Topic { get; set; }
    public ModerationStatus ModerationStatus { get; set; }
    public IEnumerable<Reaction> Reactions { get; set; }

    public AiIntegration AiIntegration { get; set; }
}