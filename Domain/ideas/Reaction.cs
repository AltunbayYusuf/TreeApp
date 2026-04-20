using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.ideas;

public class Reaction
{
    public int Id { get; set; }
    public ModerationStatus ModerationStatus { get; set; }

    [MaxLength(200)] public string Text { get; set; }
    public string Emoji { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    [Required(ErrorMessage = "Reaction must belong to an idea")]
    public Idea Idea { get; set; }
}