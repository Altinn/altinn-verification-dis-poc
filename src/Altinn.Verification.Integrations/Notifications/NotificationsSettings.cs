namespace Altinn.Verification.Integrations.Notifications;

/// <summary>
/// Settings for the notifications API client.
/// </summary>
public class NotificationsSettings
{
    /// <summary>
    /// Gets or sets the base URL for the notifications API.
    /// </summary>
    public string ApiNotificationsEndpoint { get; set; } = string.Empty;
}
