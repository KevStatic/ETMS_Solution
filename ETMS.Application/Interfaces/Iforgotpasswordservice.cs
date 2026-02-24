using ETMS.Application.DTOs;

namespace ETMS.Application.Interfaces
{
    public interface IForgotPasswordService
    {
        Task<(bool Success, string Message)> SendOtpAsync(SendOtpDto dto);
        Task<(bool Success, string Message, string? ResetToken)> VerifyOtpAsync(VerifyOtpDto dto);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
    }
}