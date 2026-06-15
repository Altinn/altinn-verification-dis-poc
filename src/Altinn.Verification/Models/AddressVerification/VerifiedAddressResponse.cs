using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Altinn.Verification.Core.AddressVerifications.Models;

namespace Altinn.Verification.Models.AddressVerification;

/// <summary>
/// Represents a verified address response containing the address value and its type.
/// </summary>
public class VerifiedAddressResponse
{
    /// <summary>
    /// Gets or sets the verified address value (email or phone number).
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string Value { get; init; }

    /// <summary>
    /// Gets or sets the type of the verified address (email or phone).
    /// </summary>
    public AddressType Type { get; init; }
}
