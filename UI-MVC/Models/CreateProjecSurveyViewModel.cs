using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models;

public class CreateProjecSurveyViewModel
{
    [Required(ErrorMessage = "ProjectSurvey must belong to a subplatform")]
    public string SubplatformSlug { get; set; } = "";

    public string SurveyJson { get; set; } = "";

    public int ProjectId { get; set; }
    
    [Required(ErrorMessage = "Projectsurvey must have at least one section")]
    public List<SectionViewModel> Sections { get; set; } = new();
}