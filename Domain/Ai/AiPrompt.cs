namespace IntegratieProject.BL.Domain.Ai;

public class AiPrompt
{
    public int Id { get; set; }

    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string PromptText { get; set; } = "";

    public bool IsActive { get; set; } = true;
}