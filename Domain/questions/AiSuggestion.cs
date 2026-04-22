using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.Domain.questions;

public class AiSuggestion
{
    public int Id { get; set; }
    public string SuggestionAnswer { get; set; }
    public Status SuggestionStatus { get; set; }
}