using Altinn.Verification.Core.AddressVerifications.Models;
using Altinn.Verification.Core.Integrations;

namespace Altinn.Verification.Integrations.Notifications;

/// <summary>
/// Sends user-facing notifications related to address verification codes.
/// Handles language resolution, message content building (via <see cref="UserMessageBuilder"/>),
/// and delivery (via <see cref="INotificationsClient"/>).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserNotifier"/> class.
/// </remarks>
/// <param name="notificationsClient">The notifications client for sending SMS and email.</param>
/// <param name="userLanguageService">The user language service for resolving preferred language.</param>
public class UserNotifier(INotificationsClient notificationsClient, IUserLanguageService userLanguageService) : IUserNotifier
{
    private readonly INotificationsClient _notificationsClient = notificationsClient;
    private readonly IUserLanguageService _userLanguageService = userLanguageService;

    /// <inheritdoc/>
    public async Task<bool> SendVerificationCodeAsync(int userId, string address, AddressType addressType, string verificationCode, CancellationToken cancellationToken)
    {
        var language = await _userLanguageService.GetPreferredLanguageAsync(userId, cancellationToken);
        var sendersReference = $"verification-{userId}-{addressType}-{DateTime.UtcNow.Ticks}";

        if (addressType == AddressType.Sms)
        {
            var phoneNumberWithCountryCode = EnsureCountryCodeIfValidNumber(address);
            var body = UserMessageBuilder.GetSmsContent(language, verificationCode);
            return await _notificationsClient.OrderSmsAsync(phoneNumberWithCountryCode, body, sendersReference, cancellationToken);
        }
        else
        {
            var subject = UserMessageBuilder.GetEmailSubject(language);
            var body = UserMessageBuilder.GetEmailBody(language, verificationCode);
            return await _notificationsClient.OrderEmailAsync(address, subject, body, sendersReference, cancellationToken);
        }
    }

    /// <summary>
    /// Checks if number contains country code; if not, adds the Norwegian country code if number starts with 4 or 9.
    /// </summary>
    /// <remarks>
    /// This method does not validate the number, only ensures that it has a country code.
    /// </remarks>
    /// <param name="mobileNumber">The mobile number to check.</param>
    /// <returns>The mobile number with a country code.</returns>
    public static string EnsureCountryCodeIfValidNumber(string mobileNumber)
    {
        if (string.IsNullOrEmpty(mobileNumber))
        {
            return mobileNumber;
        }
        else if (mobileNumber.StartsWith("00", StringComparison.Ordinal))
        {
            mobileNumber = "+" + mobileNumber.Remove(0, 2);
        }
        else if (mobileNumber.Length == 8 && (mobileNumber[0] == '9' || mobileNumber[0] == '4'))
        {
            mobileNumber = "+47" + mobileNumber;
        }

        return mobileNumber;
    }
}
