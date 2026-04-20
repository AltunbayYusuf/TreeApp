using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;

namespace IntergratieProject.UI.MVC.Models.Dto;

public class NewProjectDto
{
    public string Introduction { get; set; }
    public Status Status { get; set; }
    public string Prompt { get; set; }
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }

    public ProjectType Type { get; set; }
    public Media Photo { get; set; }
    public Media Logo { get; set; }
    public IEnumerable<Topic> Topics { get; set; } = new List<Topic>();
    public QuestionList QuestionList { get; set; }
}