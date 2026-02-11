<<<<<<< HEAD
﻿using ETMS.Application.Interfaces;
using ETMS.Application.DTOs.Auth; // Update to use the correct namespace


namespace ETMS.Application.Interfaces
//namespace ETMS.Application.Services
{

=======
﻿using System;
using System.Threading.Tasks;
using ETMS.Application.DTOs.Auth;

namespace ETMS.Application.Services
{
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
    public class AuthService : IAuthService
    {
        private readonly IUserAccountRepository _userRepository;

<<<<<<< HEAD
       

        public AuthService(IUserAccountRepository userRepository) => _userRepository = userRepository;

        public async Task<LoginResultDto> AuthenticateAsync(LoginRequestDto request)
        {
=======
        public AuthService(IUserAccountRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<LoginResultDto> AuthenticateAsync(LoginRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            // Use proper password hashing in production (e.g., BCrypt or Identity PasswordHasher)
            if (user == null || user.Password != request.Password)
<<<<<<< HEAD
                return new LoginResultDto { Success = false, ErrorMessage = "Invalid username or password." };

            if (!user.IsActive)
                return new LoginResultDto { Success = false, ErrorMessage = "Account is inactive." };
=======
            {
                return new LoginResultDto { Success = false, ErrorMessage = "Invalid username or password." };
            }

            if (!user.IsActive)
            {
                return new LoginResultDto { Success = false, ErrorMessage = "Account is inactive." };
            }
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e

            return new LoginResultDto
            {
                Success = true,
                Username = user.Username,
                Role = user.Role,
<<<<<<< HEAD
                EmployeeId = user.EmployeeId
=======
                EmployeeId = user.EmployeeId,
                ErrorMessage = null
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
            };
        }
    }
}
