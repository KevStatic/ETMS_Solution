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

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== LOGIN ATTEMPT ===");
            Console.WriteLine($"Username received: '{request.Username}'");
            Console.WriteLine($"Password received: '{request.Password}'");
            Console.ResetColor();

            var user = await _userRepository.GetByUsernameAsync(request.Username);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"User found in DB: {user != null}");
            if (user != null)
            {
                Console.WriteLine($"DB Username: '{user.Username}'");
                Console.WriteLine($"DB Password: '{user.Password}'");
                Console.WriteLine($"IsActive: {user.IsActive}");
                Console.WriteLine($"Role: {user.Role}");
            }
            Console.ResetColor();

            if (user == null)
                return new LoginResultDto { Success = false, ErrorMessage = "Invalid username or password." };

            // Plain text comparison (passwords are stored as plain text in DB)
            bool passwordValid = request.Password == user.Password;

            Console.ForegroundColor = passwordValid ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"Password valid: {passwordValid}");
            Console.ResetColor();

            if (!passwordValid)
                return new LoginResultDto { Success = false, ErrorMessage = "Invalid username or password." };

            if (!user.IsActive)
                return new LoginResultDto { Success = false, ErrorMessage = "Account is inactive." };

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== LOGIN SUCCESS ===");
            Console.ResetColor();

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