using BCrypt.Net;
using ETMS.Application.DTOs.Auth;
using ETMS.Application.Interfaces;

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

            bool passwordValid = VerifyPassword(request.Password, user.Password);

            if (!passwordValid)
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

        /// <summary>
        /// Verifies a password against a stored BCrypt hash. If the stored value is not a
        /// valid BCrypt hash (e.g. seed data that was never migrated), the credentials are
        /// treated as invalid instead of surfacing a 500 to the login page.
        /// </summary>
        private static bool VerifyPassword(string plainText, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainText, storedHash);
            }
            catch (SaltParseException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
