using ETMS.Domain.Entities;
using System.Threading.Tasks;

namespace ETMS.Application.Interfaces
{
    public interface IUserAccountRepository
    {
        Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
