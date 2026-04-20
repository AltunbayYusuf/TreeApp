namespace IntergratieProject.UI.MVC.Models;

public class SubAdminDashboardViewModel
{
    public int SubPlatformId { get; set; }
    public string SubPlatformName { get; set; } = "";
    public string Slug { get; set; } = "";
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int ParticipantsCount { get; set; }
    public int TotalIdeas { get; set; }
    public List<ProjectSummaryViewModel> Projects { get; set; } = new();
    
   
}

public class ProjectSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Form { get; set; }
    public int ParticipantsCount { get; set; }
    public int IdeasCount { get; set; }
    public DateTime ReleaseDate { get; set; }
}
