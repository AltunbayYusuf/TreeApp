 namespace IntegratieProject.UI.MVC.Models;

public class ProjectSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Form { get; set; }
    
    public bool HasBeenActive { get; set; }

    public int ParticipantsCount { get; set; }
    public int IdeasCount { get; set; }
    public DateTime ReleaseDate { get; set; }
}