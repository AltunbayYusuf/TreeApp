namespace IntegratieProject.UI.MVC.Models.Admin;

public class SubPlatformAdminOverviewViewModel
{
    public string SubPlatformName { get; set; }
    public string SubPlatformSlug { get; set; }
    public List<SubPlatformAdminViewModel> Admins { get; set; } = new();
}

public class SubPlatformAdminViewModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
}