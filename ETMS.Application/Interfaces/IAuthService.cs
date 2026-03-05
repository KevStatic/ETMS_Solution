using System.Threading.Tasks;
using ETMS.Application.DTOs.Auth;

namespace ETMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResultDto> AuthenticateAsync(LoginRequestDto request);
    }
}
