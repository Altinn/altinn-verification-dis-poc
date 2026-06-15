namespace Altinn.Verification.Core
{
    /// <summary>
    /// Configuration settings for address verification.
    /// </summary>
    public class AddressMaintenanceSettings
    {
        /// <summary>
        /// The time interval for which a user should be prevented in requesting new verification codes.
        /// </summary>
        public int VerificationCodeResendCooldownSeconds { get; set; } = 60;
    }
}
