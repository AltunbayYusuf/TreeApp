namespace IntergratieProject.UI.MVC.Models;

public class SubAdminDashboardViewModel
{
    public int SubPlatformId { get; set; }
    public string SubPlatformName { get; set; } = "";
    public string Slug { get; set; } = "";
    public List<ProjectSummaryViewModel> Projects { get; set; } = new();
}

public class ProjectSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
}