using Altinn.Verification.Core.Integrations;

namespace Altinn.Verification.Integrations.UserLanguage;

/// <summary>
/// A stub implementation of <see cref="IUserLanguageService"/> that always returns Norwegian Bokmål.
/// </summary>
/// <remarks>
/// POC simplification: a real implementation would resolve the preferred language from the user profile.
/// </remarks>
public class DefaultUserLanguageService : IUserLanguageService
{
    /// <inheritdoc/>
    public Task<string> GetPreferredLanguageAsync(int userId, CancellationToken ct)
    {
        return Task.FromResult("nb");
    }
}
