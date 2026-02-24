using ETMS.Domain.Entities;

namespace ETMS.Domain.Interfaces
{
    // Place this file in: ETMS.Domain > Interfaces > IOtpRepository.cs

    public interface IOtpRepository
    {
        Task<int> CreateAsync(OtpRequest otp);
        Task<OtpRequest?> GetLatestByEmailAsync(string email);
        Task<OtpRequest?> GetLatestByPhoneAsync(string phone);
        Task MarkVerifiedAsync(int id);
        Task MarkUsedAsync(int id);
        Task InvalidatePreviousAsync(string contact);
    }

    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);
    }

    public interface ISmsService
    {
        Task SendOtpSmsAsync(string toPhone, string otpCode);
    }
}