namespace IntergratieProject.UI.MVC.Models;

public class SubAdminDashboardViewModel
{
    public int SubPlatformId { get; set; }
    public string SubPlatformName { get; set; } = "";
    public string Slug { get; set; } = "";
    public List<ProjectSummaryViewModel> Projects { get; set; } = new();
    
    public List<IdeaReviewSummaryViewModel> IdeasReviews{get; set;} = new();
    public List<ReactionReviewSummaryViewModel> ReactionsReviews { get; set; } = new();
}

public class ProjectSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
}
public class IdeaReviewSummaryViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public string TopicTheme { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string ModerationStatus { get; set; } = "";
}

public class ReactionReviewSummaryViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string IdeaTitle { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string ModerationStatus { get; set; } = "";
}