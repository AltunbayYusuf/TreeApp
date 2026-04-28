using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models;

public class CreateProjecSurveyViewModel
{
    [Required(ErrorMessage = "ProjectSurvey must belong to a subplatform")]
    public string SubplatformSlug { get; set; } = "";

    public int ProjectId { get; set; }
    public List<SectionViewModel> Sections { get; set; } = new();
    
    public string SurveyJson { get; set; } = "";
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Sections == null || !Sections.Any())
        {
            yield return new ValidationResult(
                "Projectsurvey must have at least one section", new[] { nameof(Sections) });
        }
    }
}