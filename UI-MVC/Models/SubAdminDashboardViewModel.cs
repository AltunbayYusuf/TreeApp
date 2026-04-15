namespace IntergratieProject.UI.MVC.Models;

public class SubAdminDashboardViewModel
{
    public int SubPlatformId { get; set; }
    public string SubPlatformName { get; set; } = "";
    public string Slug { get; set; } = "";
    public List<ProjectSummaryViewModel> Projects { get; set; } = new();
    
    public int TotalProjects => Projects.Count;
    public int ActiveProjects => Projects.Count(p => p.Status == "Active");
    public int TotalParticipants => Projects.Sum(p => p.ParticipantsCount);
    public int TotalIdeas => Projects.Sum(p => p.IdeasCount);
}

