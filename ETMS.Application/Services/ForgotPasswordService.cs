using System.Security.Cryptography;
using ETMS.Application.DTOs;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using BC = BCrypt.Net.BCrypt;

namespace ETMS.Application.Services
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly IOtpRepository _otpRepo;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly IUserAccountRepository _userAccountRepo;

        public ForgotPasswordService(
            IOtpRepository otpRepo,
            IEmailService emailService,
            IMemoryCache cache,
            IUserAccountRepository userAccountRepo)
        {
            _otpRepo = otpRepo;
            _emailService = emailService;
            _cache = cache;
            _userAccountRepo = userAccountRepo;
        }

        // ─── STEP 1: Send OTP ─────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> SendOtpAsync(SendOtpDto dto)
        {
            bool exists = await _userAccountRepo.EmailExistsAsync(dto.Email);
            if (!exists)
                return (false, "No account found with this email. Please check and try again.");

            await _otpRepo.InvalidatePreviousAsync(dto.Email);

            string rawOtp = GenerateOtp();
            string hashedOtp = BC.HashPassword(rawOtp);

            var record = new OtpRequest
            {
                Email = dto.Email,
                OtpCode = hashedOtp,
                Channel = "email",
                ExpiresAt = DateTime.UtcNow.AddMinutes(2),
            };

            await _otpRepo.CreateAsync(record);

            try
            {
                await _emailService.SendOtpEmailAsync(dto.Email, rawOtp);
            }
            catch (Exception)
            {
                // Don't leak SMTP/internal error details to the UI.
                return (false, "Failed to send OTP. Please try again in a moment.");
            }

            return (true, "OTP sent to your email. Valid for 2 minutes.");
        }

        // ─── STEP 2: Verify OTP ───────────────────────────────────────────────
        public async Task<(bool Success, string Message, string? ResetToken)> VerifyOtpAsync(VerifyOtpDto dto)
        {
            OtpRequest? record = await _otpRepo.GetLatestByEmailAsync(dto.Email);

            if (record == null || record.IsUsed)
                return (false, "OTP not found or already used. Please request a new one.", null);

            if (record.ExpiresAt < DateTime.UtcNow)
                return (false, "OTP has expired. Please request a new one.", null);

            bool valid = BC.Verify(dto.OtpCode, record.OtpCode);
            if (!valid)
                return (false, "Invalid OTP. Please check and try again.", null);

            await _otpRepo.MarkVerifiedAsync(record.Id);

            string resetToken = GenerateResetToken();
            _cache.Set($"reset_{resetToken}", dto.Email, TimeSpan.FromMinutes(10));

            return (true, "OTP verified successfully!", resetToken);
        }

        // ─── STEP 3: Reset Password ───────────────────────────────────────────
        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            string cacheKey = $"reset_{dto.ResetToken}";

            if (!_cache.TryGetValue(cacheKey, out string? email) || email == null)
                return (false, "Session expired. Please start the reset process again.");

            if (!IsPasswordStrong(dto.NewPassword))
                return (false, "Password must have 8+ characters, uppercase, lowercase, number, and a symbol.");

            string hashedPassword = BC.HashPassword(dto.NewPassword);
            // NEW
            await _userAccountRepo.UpdatePasswordAsync(email, hashedPassword);

            _cache.Remove(cacheKey);

            return (true, "Password reset successfully! You can now log in.");
        }

        // ─── Helpers ──────────────────────────────────────────────────────────
        private static string GenerateOtp() =>
            RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        private static string GenerateResetToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                   .Replace("+", "-").Replace("/", "_").Replace("=", "");

        private static bool IsPasswordStrong(string pw) =>
            pw.Length >= 8 &&
            pw.Any(char.IsUpper) &&
            pw.Any(char.IsLower) &&
            pw.Any(char.IsDigit) &&
            pw.Any(c => !char.IsLetterOrDigit(c));
    }
}