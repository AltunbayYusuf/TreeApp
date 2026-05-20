using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.BL.Interfaces;

public interface IAiModerationService
{
    Task<ToxicityResult> ModerateIdeaAsync(string title, string text, int? subPlatformId = null);
    Task<ToxicityResult> ModerateReactionAsync(string text, int? subPlatformId = null);
}