using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.Questions;

public class Answer
{
    public int Id { get; set; }
    public string Text { get; set; }
    public User User  { get; set; }
    public Question Question { get; set; }
}