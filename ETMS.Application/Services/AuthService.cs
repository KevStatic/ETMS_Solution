using ETMS.Application.Interfaces;
using ETMS.Application.DTOs.Auth; // Update to use the correct namespace


namespace ETMS.Application.Interfaces
//namespace ETMS.Application.Services
{

    public class AuthService : IAuthService
    {
        private readonly IUserAccountRepository _userRepository;

       

        public AuthService(IUserAccountRepository userRepository) => _userRepository = userRepository;

        public async Task<LoginResultDto> AuthenticateAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            // Use proper password hashing in production (e.g., BCrypt or Identity PasswordHasher)
            if (user == null || user.Password != request.Password)
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
