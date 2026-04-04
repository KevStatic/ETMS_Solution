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
        Task<bool> UpdatePasswordByIdAsync(int userAccountId, string newPassword);
    }
}