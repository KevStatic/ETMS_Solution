using System.Security.Cryptography;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Domain.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace ETMS.Application.Services
{
    /// <summary>
    /// Email-OTP second factor for login. Reuses the same OTP storage and email
    /// channel as the forgot-password flow, but issues codes for an authenticated
    /// (password-verified) login attempt rather than a password reset.
    /// </summary>
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IOtpRepository _otpRepo;
        private readonly IEmailService _emailService;

        // Login codes are short-lived; long enough to read an email, short enough to be safe.
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);

        public TwoFactorService(IOtpRepository otpRepo, IEmailService emailService)
        {
            _otpRepo = otpRepo;
            _emailService = emailService;
        }

        public async Task<(bool Success, string Message)> SendLoginOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "No email is on file for this account, so a login code can't be sent.");

            await _otpRepo.InvalidatePreviousAsync(email);

            string rawOtp = GenerateOtp();
            var record = new OtpRequest
            {
                Email = email,
                OtpCode = BC.HashPassword(rawOtp),
                Channel = "email",
                ExpiresAt = DateTime.UtcNow.Add(OtpLifetime)
            };
            await _otpRepo.CreateAsync(record);

            try
            {
                await _emailService.SendOtpEmailAsync(email, rawOtp);
            }
            catch (Exception)
            {
                return (false, "We couldn't send your login code. Please try again in a moment.");
            }

            return (true, $"A 6-digit login code was sent to {MaskEmail(email)}. It expires in 5 minutes.");
        }

        public async Task<(bool Success, string Message)> VerifyLoginOtpAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Please enter the code from your email.");

            var record = await _otpRepo.GetLatestByEmailAsync(email);

            if (record == null || record.IsUsed)
                return (false, "This code is no longer valid. Please sign in again to get a new one.");

            if (record.ExpiresAt < DateTime.UtcNow)
                return (false, "Your login code has expired. Please sign in again to get a new one.");

            if (!BC.Verify(code, record.OtpCode))
                return (false, "Incorrect code. Please check your email and try again.");

            await _otpRepo.MarkUsedAsync(record.Id);
            return (true, "Verified.");
        }

        private static string GenerateOtp() =>
            RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        private static string MaskEmail(string email)
        {
            int at = email.IndexOf('@');
            if (at <= 1) return email;
            return $"{email[0]}{new string('*', Math.Min(at - 1, 5))}{email.Substring(at)}";
        }
    }
}
