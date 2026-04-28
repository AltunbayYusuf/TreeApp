using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models;

public class GenerateSurveyRequest
{
    [Required]
    [MinLength(50, ErrorMessage = "Beschrijving moet minstens 50 tekens bevatten.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Aantal vragen moet tussen 1 en 20 liggen.")]
    public int QuestionAmount { get; set; }
}