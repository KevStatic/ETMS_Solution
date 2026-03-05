using System.ComponentModel.DataAnnotations;

namespace ETMS.Application.DTOs
{
    // ── Step 1: Send OTP ──────────────────────────────────────────────────────
    public class SendOtpDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;
    }

    // ── Step 2: Verify OTP ────────────────────────────────────────────────────
    public class VerifyOtpDto
    {
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        public string OtpCode { get; set; } = string.Empty;
    }

    // ── Step 3: Reset Password ────────────────────────────────────────────────
    public class ResetPasswordDto
    {
        [Required]
        public string ResetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}