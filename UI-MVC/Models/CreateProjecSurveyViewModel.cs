namespace IntergratieProject.UI.MVC.Models;

public class CreateProjecSurveyViewModel
{
    public string SubplatformSlug { get; set; } = "";

    public int ProjectId { get; set; }

    public List<SectionViewModel> Sections { get; set; } = new();
}
