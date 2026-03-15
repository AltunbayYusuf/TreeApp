using IntergratieProject.Domain.ideas;

namespace IntergratieProject.Domain.Questions;

public class Question
{
    public int Id { get; set; }
    public string Description { get; set; }
    public Section Section { get; set; }
    public QuestionType QuestionType { get; set; }
    public Media Image { get; set; }
    public List<Answer> Answers { get; set; }
    

}