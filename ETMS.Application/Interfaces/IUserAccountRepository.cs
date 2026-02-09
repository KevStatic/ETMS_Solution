using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface IUserAccountRepository
    {
        Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}

