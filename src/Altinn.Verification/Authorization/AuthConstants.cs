namespace Altinn.Verification.Authorization;

/// <summary>
/// Constants for authorization policy names used in the Verification API.
/// </summary>
public static class AuthConstants
{
    /// <summary>
    /// Policy name for authenticated end-users accessing the portal.
    /// Requires the <c>altinn:portal/enduser</c> scope.
    /// </summary>
    public const string PortalEndUserAccess = "PortalEndUserAccess";
}
