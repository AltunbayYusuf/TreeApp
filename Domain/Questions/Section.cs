namespace IntergratieProject.Domain.Questions;

public class Section
{
    public int SectionId { get; set; }
    public String Title { get; set; }
    public int Order { get; set; }
    public List<Question> Type { get; set; }
}