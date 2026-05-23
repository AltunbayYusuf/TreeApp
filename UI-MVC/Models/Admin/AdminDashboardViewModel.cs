namespace IntegratieProject.UI.MVC.Models.Admin;

public class AdminDashboardViewModel
{
    public int SubPlatformCount { get; set; }
    public int SubAdminCount { get; set; }
    public int ProjectCount { get; set; }
    public int ParticipantCount { get; set; }
    public List<AdminDashboardSubPlatformViewModel> SubPlatforms { get; set; } = new();
}

public class AdminDashboardSubPlatformViewModel
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string Initial { get; set; }
    public int SubAdminCount { get; set; }
    public int ProjectCount { get; set; }
    public int ParticipantCount { get; set; }
}
