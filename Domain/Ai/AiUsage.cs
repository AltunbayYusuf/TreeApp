namespace IntegratieProject.BL.Domain.Ai;

public class AiUsage
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Feature { get; set; } = "";
    public string Model { get; set; } = "";
    public bool Success { get; set; }

    public string ErrorMessage { get; set; }
}