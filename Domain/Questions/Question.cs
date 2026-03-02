namespace IntergratieProject.Domain.Questions;

public class Question
{
    public int questionId { get; set; }
    public string description { get; set; }
    public QuestionType QuestionType { get; set; }
    //public Media Image { get; set; }
    public List<Answer> Answers { get; set; }
}