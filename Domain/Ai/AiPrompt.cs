using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.BL.Domain.Ai;

public class AiPrompt
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string Key { get; set; } = "";
    [Required] [MaxLength(100)] public string Name { get; set; } = "";
    [Required] [MaxLength(4000)] public string PromptText { get; set; } = "";

    public bool IsActive { get; set; } = true;
}