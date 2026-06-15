using System.Security.Cryptography;

using Altinn.Verification.Core.AddressVerifications.Models;
using Altinn.Verification.Core.Integrations;

namespace Altinn.Verification.Integrations.AddressVerification;

/// <summary>
/// Service for creating and verifying verification codes using BCrypt hashing.
/// </summary>
public class VerificationCodeService : IVerificationCodeService
{
    /// <inheritdoc/>
    public string GenerateRawCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }

    /// <inheritdoc/>
    public VerificationCode CreateVerificationCode(int userId, string address, AddressType addressType, string verificationCode)
    {
        var hashedCode = BCrypt.Net.BCrypt.HashPassword(verificationCode);

        return new VerificationCode
        {
            UserId = userId,
            Address = VerificationCode.FormatAddress(address),
            AddressType = addressType,
            VerificationCodeHash = hashedCode,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(15),
        };
    }

    /// <inheritdoc/>
    public bool VerifyCode(string code, VerificationCode verificationCode)
    {
        if (verificationCode.Expires < DateTime.UtcNow)
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(code, verificationCode.VerificationCodeHash);
    }
}
