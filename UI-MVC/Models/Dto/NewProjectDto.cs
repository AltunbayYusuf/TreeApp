using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.UI.MVC.Models.Dto;

public class NewProjectDto
{
    [Required(ErrorMessage = "Project must have an introduction")]
    public string Introduction { get; set; }
    [Required(ErrorMessage = "Project must have a status")]
    public Status Status { get; set; }
    public string Prompt { get; set; }
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }

    public ProjectType Type { get; set; }
    public Media Photo { get; set; }
    public Media Logo { get; set; }
    [Required(ErrorMessage = "Project must have topics")]
    public IEnumerable<Topic> Topics { get; set; } = new List<Topic>();
    [Required(ErrorMessage = "Project must have questions")]
    public QuestionList QuestionList { get; set; }
}