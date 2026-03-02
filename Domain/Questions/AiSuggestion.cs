namespace IntergratieProject.Domain.Questions;

public class AiSuggestion
{
    public int SuggestionId { get; set; }
    public string SugestionAnswer { get; set; }
    public Status SuggestionStatus { get; set; }
}