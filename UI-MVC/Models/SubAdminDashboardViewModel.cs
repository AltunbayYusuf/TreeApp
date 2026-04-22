using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models;

public class SubAdminDashboardViewModel
{
    [Required(ErrorMessage = "SubAdmin dashboard must belong to a subplatform")]
    public int SubPlatformId { get; set; }
    public string SubPlatformName { get; set; } = "";
    public string Slug { get; set; } = "";
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int ParticipantsCount { get; set; }
    public int TotalIdeas { get; set; }
    
    public Boolean IsAlActiefGeweest { get; set; }
    public List<ProjectSummaryViewModel> Projects { get; set; } = new();
    
   
}


