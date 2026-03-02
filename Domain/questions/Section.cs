namespace IntergratieProject.Domain.Questions;

public class Section
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Order { get; set; }
    public List<Question> Type { get; set; }
}