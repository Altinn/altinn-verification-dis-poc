using System.ComponentModel.DataAnnotations;

namespace Altinn.Verification.Validators;

/// <summary>
/// Custom regex attribute for notification address validation.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CustomRegexForNotificationAddressesAttribute : RegularExpressionAttribute
{
    private const string _emailRegexPattern = @"^((""[^""]+"")|(([a-zA-Z0-9!#$%&'*+\-=?\^_`{|}~])+(\.([a-zA-Z0-9!#$%&'*+\-=?\^_`{|}~])+)*))@((((([a-zA-Z0-9æøåÆØÅ]([a-zA-Z0-9\-æøåÆØÅ]{0,61})[a-zA-Z0-9æøåÆØÅ]\.)|[a-zA-Z0-9æøåÆØÅ]\.){1,9})([a-zA-Z]{2,14}))|((\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})))$";
    private const string _internationalPhoneRegexPattern = @"^(((\+[0-9]{2})[0-9]+))$";

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRegexForNotificationAddressesAttribute"/> class.
    /// </summary>
    /// <param name="inputType">The type of input to validate.</param>
    public CustomRegexForNotificationAddressesAttribute(string inputType)
        : base(GetRegex(inputType))
    {
    }

    private static string GetRegex(string inputType)
    {
        return inputType switch
        {
            ValidationRule.EmailAddress => _emailRegexPattern,
            ValidationRule.InternationalPhoneNumber => _internationalPhoneRegexPattern,
            _ => throw new ArgumentException($"Unknown input type: {inputType}", nameof(inputType)),
        };
    }
}

/// <summary>
/// Constants for validation rule types used with <see cref="CustomRegexForNotificationAddressesAttribute"/>.
/// </summary>
public static class ValidationRule
{
    /// <summary>
    /// Email address validation rule.
    /// </summary>
    public const string EmailAddress = "EmailAddress";

    /// <summary>
    /// Phone number with international country code validation rule.
    /// </summary>
    public const string InternationalPhoneNumber = "InternationalPhoneNumber";
}
