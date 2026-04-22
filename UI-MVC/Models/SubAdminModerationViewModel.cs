namespace IntegratieProject.UI.MVC.Models;

public class SubAdminModerationViewModel
{

    public string ActiveFilter { get; set; } = "algemeen";
    public List<ModerationQueueItemViewModel> Items { get; set; } = new();
    public ModerationQueueItemViewModel SelectedItem { get; set; }
}

public class ModerationQueueItemViewModel
{
    public string Type { get; set; } = ""; // idea | reaction
    public int Id { get; set; }

    public string Label { get; set; } = ""; // Idee | Reactie
    public string Title { get; set; } = "";
    public string PreviewText { get; set; } = "";
    public string ContentText { get; set; } = "";

    public string ProjectName { get; set; } = "";
    public string TopicTheme { get; set; } = "";
    public string IdeaTitle { get; set; } = "";
    public string ModerationReason { get; set; } = "Mogelijk ongepaste taal gedetecteerd door AI";

    public string ModerationStatus { get; set; } = "";
    public bool IsSelected { get; set; }
}