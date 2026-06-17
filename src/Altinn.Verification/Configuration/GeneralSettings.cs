namespace Altinn.Verification.Configuration;

/// <summary>
/// General application settings for the Verification API.
/// </summary>
public class GeneralSettings
{
    /// <summary>
    /// Gets or sets the OpenID Connect well-known endpoint URL used for JWT validation.
    /// </summary>
    public string OpenIdWellKnownEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the JWT cookie used for authentication.
    /// </summary>
    public string JwtCookieName { get; set; } = string.Empty;
}
