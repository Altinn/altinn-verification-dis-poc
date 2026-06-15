using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Altinn.Verification.Core.AddressVerifications.Models;

namespace Altinn.Verification.Models.AddressVerification;

/// <summary>
/// Represents a request to verify an address using a verification code.
/// </summary>
public class AddressVerificationRequest
{
    /// <summary>
    /// Gets or sets the address to verify, either an email or a phone number.
    /// </summary>
    [Required]
    [JsonRequired]
    [StringLength(320)]
    [MinLength(5)]
    public required string Value { get; init; }

    /// <summary>
    /// Gets or sets the type of the address, either "email" or "sms".
    /// </summary>
    [Required]
    [JsonRequired]
    public required AddressType? Type { get; init; }

    /// <summary>
    /// Gets or sets the 6-digit verification code for the address.
    /// </summary>
    [Required]
    [JsonRequired]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$")]
    public required string VerificationCode { get; init; }
}
