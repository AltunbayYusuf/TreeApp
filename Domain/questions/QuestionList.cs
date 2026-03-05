using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.Questions;

public class QuestionList
{
    public int Id { get; set; }

    public List<Section> Sections { get; set; }
    public Project Project  { get; set; }
}