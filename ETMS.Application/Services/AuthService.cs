using ETMS.Application.DTOs.Auth;
using ETMS.Application.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace ETMS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserAccountRepository _userRepository;

        public AuthService(IUserAccountRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<LoginResultDto> AuthenticateAsync(LoginRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null)
                return new LoginResultDto { Success = false, ErrorMessage = "Invalid username or password." };

            // ✅ BCrypt verify - matches what ResetPassword saves
            if (!BC.Verify(request.Password, user.Password))
                return new LoginResultDto { Success = false, ErrorMessage = "Invalid username or password." };

            if (!user.IsActive)
                return new LoginResultDto { Success = false, ErrorMessage = "Account is inactive." };

            return new LoginResultDto
            {
                Success = true,
                Username = user.Username,
                Role = user.Role,
                EmployeeId = user.EmployeeId
            };
        }
    }
}