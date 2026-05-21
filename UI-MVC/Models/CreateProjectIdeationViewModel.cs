using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.UI.MVC.Models;

public class CreateProjectIdeationViewModel
{
    public string SubplatformSlug { get; set; } = "";

    public List<ExistingTopicOptionViewModel> ExistingTopics { get; set; } = new();

    [Required]
    [MinLength(1, ErrorMessage = "Er moet minstens 1 topic zijn.")]
    [MaxLength(5, ErrorMessage = "Je kan maximaal 5 topics toevoegen.")]
    public List<IdeationTopicViewModel> Topics { get; set; } = new()
    {
        new IdeationTopicViewModel()
    };

    public string ProjectContext { get; set; } = "";

    public string IdeaInfo { get; set; } = "";

    [Range(3, 10, ErrorMessage = "Aantal ideeën per keer moet tussen 3 en 10 liggen.")]
    public int IdeasPerBatch { get; set; } = 3;

    [Range(0, 10, ErrorMessage = "Max keer extra opvragen moet tussen 0 en 10 liggen.")]
    public int MaxExtraRequests { get; set; } = 2;

    public string SelectedEmojiGroup { get; set; } = "👍,❤️";

    public bool EnableContactOptIn { get; set; }
}

public class IdeationTopicViewModel
{
    [Required(ErrorMessage = "Een topic titel is verplicht.")]
    [MaxLength(50, ErrorMessage = "Topic titel mag maximaal 50 tekens zijn.")]
    public string Title { get; set; } = "";

    [MaxLength(200, ErrorMessage = "Beschrijving mag maximaal 200 tekens zijn.")]
    public string Description { get; set; } = "";
}

public class ExistingTopicOptionViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";
}
