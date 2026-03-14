namespace IntergratieProject.Models.Dto;

public class ReactionDto
{
    public int Id { get; set; }
    public int IdeaId { get; set; }
    public string Emoji { get; set; }
    public string Text { get; set; }
}