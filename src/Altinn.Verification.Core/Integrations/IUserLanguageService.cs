namespace Altinn.Verification.Core.Integrations
{
    /// <summary>
    /// Provides the preferred language for a given user, used for localizing notification content.
    /// </summary>
    public interface IUserLanguageService
    {
        /// <summary>
        /// Retrieves the preferred language code for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A language code such as "nb", "nn", or "en".</returns>
        Task<string> GetPreferredLanguageAsync(int userId, CancellationToken cancellationToken);
    }
}
