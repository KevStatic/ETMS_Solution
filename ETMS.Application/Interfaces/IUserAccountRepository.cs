using System.Threading.Tasks;
using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface IUserAccountRepository
    {
        Task<User> GetByUsernameAsync(string username);
    }
}
