namespace IntegratieProject.UI.MVC.Models.Admin;

public class AdminStatsOverviewViewModel
{
    public int TotalPlatforms { get; set; }
    public int TotalAdmins { get; set; }

    public List<SubPlatformAdminOverviewViewModel> Platforms { get; set; } = new();
}