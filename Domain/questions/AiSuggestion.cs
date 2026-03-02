using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.Questions;

public class AiSuggestion
{
    public int Id { get; set; }
    public string SuggestionAnswer { get; set; }
    public Status SuggestionStatus { get; set; }
}