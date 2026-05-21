namespace IntegratieProject.UI.MVC.Models.Admin;

public class AiPromptPageViewModel
{
    public IList<AiPromptViewModel> Prompts { get; set; } = new List<AiPromptViewModel>();

    public IList<string> TextModels { get; set; } = new List<string>();
    public IList<string> ImageModels { get; set; } = new List<string>();

    public string SelectedTextModel { get; set; } 
    public string SelectedImageModel { get; set; } 
}