using ETMS.Domain.Entities;
using System.Threading.Tasks;

namespace ETMS.Application.Interfaces
{
    public interface IUserAccountRepository
    {
        Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email);
        Task UpdatePasswordAsync(string email, string hashedPassword);  // removed channel + phone
    }
}