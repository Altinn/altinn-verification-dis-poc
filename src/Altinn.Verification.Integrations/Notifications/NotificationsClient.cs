using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Altinn.Common.AccessTokenClient.Services;
using Altinn.Verification.Core.Integrations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Verification.Integrations.Notifications;

/// <summary>
/// A content-agnostic HTTP client for interacting with the Altinn notifications service.
/// Responsible only for HTTP transport; callers build message content.
/// </summary>
public class NotificationsClient : INotificationsClient
{
    private readonly HttpClient _httpClient;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly ILogger<NotificationsClient> _logger;
    private const string _notificationTypeSms = "sms";
    private const string _notificationTypeEmail = "email";

    private static readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make requests to the Notifications service.</param>
    /// <param name="settings">The Notifications settings containing the API endpoint.</param>
    /// <param name="accessTokenGenerator">The access token generator.</param>
    /// <param name="logger">The logger.</param>
    public NotificationsClient(HttpClient httpClient, IOptions<NotificationsSettings> settings, IAccessTokenGenerator accessTokenGenerator, ILogger<NotificationsClient> logger)
    {
        _httpClient = httpClient;
        _accessTokenGenerator = accessTokenGenerator;
        _httpClient.BaseAddress = new Uri(settings.Value.ApiNotificationsEndpoint);
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> OrderSmsAsync(string phoneNumber, string body, string? sendersReference, CancellationToken cancellationToken)
    {
        var request = new SmsOrderRequest
        {
            IdempotencyId = Guid.NewGuid().ToString(),
            SendersReference = sendersReference,
            RecipientSms = new RecipientSms
            {
                PhoneNumber = phoneNumber,
                SmsSettings = new SmsSettings
                {
                    Body = body,
                }
            }
        };

        var json = JsonSerializer.Serialize(request, _options);
        return await SendOrder(json, _notificationTypeSms, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> OrderEmailAsync(string emailAddress, string subject, string body, string? sendersReference, CancellationToken cancellationToken)
    {
        var request = new EmailOrderRequest
        {
            IdempotencyId = Guid.NewGuid().ToString(),
            SendersReference = sendersReference,
            RecipientEmail = new RecipientEmail
            {
                EmailAddress = emailAddress,
                EmailSettings = new EmailSettings
                {
                    Subject = subject,
                    Body = body,
                }
            }
        };

        var json = JsonSerializer.Serialize(request, _options);
        return await SendOrder(json, _notificationTypeEmail, cancellationToken);
    }

    private async Task<bool> SendOrder(string jsonString, string type, CancellationToken cancellationToken)
    {
        var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "verification");
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Invalid access token generated for notification order.");
            return false;
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"v1/future/orders/instant/{type}")
        {
            Content = new StringContent(jsonString, Encoding.UTF8, "application/json")
        };

        requestMessage.Headers.Add("PlatformAccessToken", accessToken);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to send order request. Status code: {StatusCode}, Response: {ResponseContent}", response.StatusCode, responseContent);
            return false;
        }

        return true;
    }
}

/// <summary>
/// Represents an SMS order request to the notifications service.
/// </summary>
internal sealed class SmsOrderRequest
{
    /// <summary>
    /// Gets or sets the idempotency ID.
    /// </summary>
    public string IdempotencyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the senders reference.
    /// </summary>
    public string? SendersReference { get; set; }

    /// <summary>
    /// Gets or sets the SMS recipient details.
    /// </summary>
    public RecipientSms RecipientSms { get; set; } = new();
}

/// <summary>
/// Represents an email order request to the notifications service.
/// </summary>
internal sealed class EmailOrderRequest
{
    /// <summary>
    /// Gets or sets the idempotency ID.
    /// </summary>
    public string IdempotencyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the senders reference.
    /// </summary>
    public string? SendersReference { get; set; }

    /// <summary>
    /// Gets or sets the email recipient details.
    /// </summary>
    public RecipientEmail RecipientEmail { get; set; } = new();
}

/// <summary>
/// Represents an SMS recipient and its settings.
/// </summary>
internal sealed class RecipientSms
{
    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMS-specific settings.
    /// </summary>
    public SmsSettings SmsSettings { get; set; } = new();
}

/// <summary>
/// Represents an email recipient and its settings.
/// </summary>
internal sealed class RecipientEmail
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email-specific settings.
    /// </summary>
    public EmailSettings EmailSettings { get; set; } = new();
}

/// <summary>
/// Represents SMS content settings.
/// </summary>
internal sealed class SmsSettings
{
    /// <summary>
    /// Gets or sets the SMS body text.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Represents email content settings.
/// </summary>
internal sealed class EmailSettings
{
    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email body.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
