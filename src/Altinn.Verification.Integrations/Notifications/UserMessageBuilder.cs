using Altinn.Verification.Core.AddressVerifications.Models;

namespace Altinn.Verification.Integrations.Notifications;

/// <summary>
/// Builds localized messages for address verification notifications.
/// </summary>
internal static class UserMessageBuilder
{
    private const string SmsBodyNb = "Bekreft e-postadresse eller telefonnummer i Altinn med koden: $code";
    private const string SmsBodyNn = "Stadfest e-postadresse eller telefonnummer i Altinn med koden: $code";
    private const string SmsBodyEn = "Confirm your email address or phone number in Altinn with the code: $code";

    private const string EmailSubjectNb = "Bekreft kontaktinformasjon i Altinn";
    private const string EmailSubjectNn = "Stadfest kontaktinformasjon i Altinn";
    private const string EmailSubjectEn = "Confirm contact information in Altinn";

    private const string EmailBodyNb = "Bruk denne koden for å bekrefte e-postadressen eller telefonnummeret ditt i Altinn:\n\n$code\n\nKoden er gyldig i 15 minutter.";
    private const string EmailBodyNn = "Bruk denne koden for å stadfeste e-postadressa eller telefonnummeret ditt i Altinn:\n\n$code\n\nKoden gjeld i 15 minuttar.";
    private const string EmailBodyEn = "Use this code to confirm your email address or phone number in Altinn:\n\n$code\n\nThe code is valid for 15 minutes.";

    /// <summary>
    /// Gets the SMS content for a verification code in the specified language.
    /// </summary>
    /// <param name="language">The language code (nb, nn, en).</param>
    /// <param name="verificationCode">The plain-text verification code.</param>
    /// <returns>The SMS body string.</returns>
    internal static string GetSmsContent(string language, string verificationCode)
    {
        var template = language switch
        {
            "nn" => SmsBodyNn,
            "en" => SmsBodyEn,
            _ => SmsBodyNb,
        };

        return template.Replace("$code", verificationCode, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the email subject for a verification notification in the specified language.
    /// </summary>
    /// <param name="language">The language code (nb, nn, en).</param>
    /// <returns>The email subject string.</returns>
    internal static string GetEmailSubject(string language)
    {
        return language switch
        {
            "nn" => EmailSubjectNn,
            "en" => EmailSubjectEn,
            _ => EmailSubjectNb,
        };
    }

    /// <summary>
    /// Gets the email body for a verification code in the specified language.
    /// </summary>
    /// <param name="language">The language code (nb, nn, en).</param>
    /// <param name="verificationCode">The plain-text verification code.</param>
    /// <returns>The email body string.</returns>
    internal static string GetEmailBody(string language, string verificationCode)
    {
        var template = language switch
        {
            "nn" => EmailBodyNn,
            "en" => EmailBodyEn,
            _ => EmailBodyNb,
        };

        return template.Replace("$code", verificationCode, StringComparison.Ordinal);
    }

    /// <summary>
    /// Ensures the phone number has a country code prefix if it's a valid Norwegian number without one.
    /// </summary>
    /// <param name="mobileNumber">The raw mobile number string.</param>
    /// <returns>The number with a country code, or the original string if it could not be parsed.</returns>
    internal static string EnsureCountryCodeIfValidNumber(string mobileNumber)
    {
        if (mobileNumber.StartsWith('+'))
        {
            return mobileNumber;
        }

        // Assume Norwegian number if no country code
        return "+47" + mobileNumber.TrimStart('0');
    }

    /// <summary>
    /// Resolves the email address display name from the address type.
    /// </summary>
    /// <param name="addressType">The address type.</param>
    /// <param name="address">The raw address value.</param>
    /// <returns>The address with a country code prefix for SMS, or as-is for email.</returns>
    internal static string NormalizeAddress(AddressType addressType, string address)
    {
        return addressType == AddressType.Sms
            ? EnsureCountryCodeIfValidNumber(address)
            : address;
    }
}
