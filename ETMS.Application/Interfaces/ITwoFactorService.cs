namespace ETMS.Application.Interfaces
{
    /// <summary>
    /// Email one-time-password second factor used at login when a user has 2FA enabled.
    /// </summary>
    public interface ITwoFactorService
    {
        /// <summary>Generates a login OTP, stores it (hashed) and emails it to the address.</summary>
        Task<(bool Success, string Message)> SendLoginOtpAsync(string email);

        /// <summary>Validates a login OTP previously sent to the address.</summary>
        Task<(bool Success, string Message)> VerifyLoginOtpAsync(string email, string code);
    }
}
