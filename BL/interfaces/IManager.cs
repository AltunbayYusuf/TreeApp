using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.BL.interfaces;

public interface IManager
{
    //Task<(bool IsToxic, string SuggestedText, string Explanation)> ModerateTextAsync(string input);
    Task<ToxicityResult> ModerateTextAsync(string input);
    // Task<ToxicityResult> SubmitReactionAsync(int ideaId, string emoji, string text);
    public void ValidateEntity(Object model);
}