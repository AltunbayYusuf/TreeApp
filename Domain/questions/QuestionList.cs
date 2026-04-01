using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.Questions;

public class QuestionList
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public List<Section> Sections { get; set; } = new();
}