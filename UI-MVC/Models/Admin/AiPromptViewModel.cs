namespace IntegratieProject.UI.MVC.Models.Admin;

public class AiPromptViewModel
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string PromptText { get; set; } = "";
}