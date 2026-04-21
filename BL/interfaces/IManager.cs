using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;

namespace IntergratieProject.BL.interfaces;

public interface IManager
{
    public IEnumerable<Topic> GetTopicsByProject(Project project);
    //Task<(bool IsToxic, string SuggestedText, string Explanation)> ModerateTextAsync(string input);
    Task<ToxicityResult> ModerateTextAsync(string input);
    // Task<ToxicityResult> SubmitReactionAsync(int ideaId, string emoji, string text);
    SubPlatform? GetSubPlatformBySlug(string slug);
    public void ValidateEntety(Object model);
}