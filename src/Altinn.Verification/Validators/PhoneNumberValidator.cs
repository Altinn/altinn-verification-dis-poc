using PhoneNumbers;

namespace Altinn.Verification.Validators;

/// <summary>
/// Provides validation functionality for phone numbers.
/// </summary>
public static class PhoneNumberValidator
{
    /// <summary>
    /// Validates the phone number using the libphonenumber library (extra validation not possible with regex).
    /// </summary>
    /// <param name="input">The phone number string to validate.</param>
    /// <returns>True if the phone number is valid; otherwise, false.</returns>
    public static bool IsValidPhoneNumber(string input)
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();

        bool isValidNumber;

        try
        {
            PhoneNumber phoneNumber = phoneNumberUtil.Parse(input, "NO");
            isValidNumber = phoneNumberUtil.IsValidNumber(phoneNumber);
        }
        catch (NumberParseException)
        {
            isValidNumber = false;
        }

        return isValidNumber;
    }
}
