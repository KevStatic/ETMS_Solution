using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface IUserAccountRepository
    {
        Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email);
        Task UpdatePasswordAsync(string email, string hashedPassword);

        // Profile methods
        Task<UserAccount?> GetByUserAccountIdAsync(int userAccountId);
        Task<UserAccount?> GetByEmployeeIdAsync(int employeeId);

        /// <summary>Resolves the email address used for OTP delivery (falls back to Username).</summary>
        Task<string?> GetLoginEmailByEmployeeIdAsync(int employeeId);

        Task<bool> UpdatePasswordByIdAsync(int userAccountId, string newPassword);
        Task<bool> UpdatePasswordByEmployeeIdAsync(int employeeId, string newPassword);
    }
}
