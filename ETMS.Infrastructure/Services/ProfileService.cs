using BCrypt.Net;
using ETMS.Application.DTOs.Profile;
using ETMS.Application.Interfaces;

namespace ETMS.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IEmployeeRepository _empRepo;
        private readonly IUserAccountRepository _userRepo;

        public ProfileService(IEmployeeRepository empRepo, IUserAccountRepository userRepo)
        {
            _empRepo = empRepo;
            _userRepo = userRepo;
        }

        public async Task<EmployeeProfileDto> GetProfileAsync(int employeeId)
        {
            var profile = await _empRepo.GetProfileByEmployeeIdAsync(employeeId);
            return profile ?? new EmployeeProfileDto();
        }

        public async Task<bool> UpdateProfileAsync(int employeeId, UpdateProfileDto dto)
        {
            return await _empRepo.UpdateProfileAsync(employeeId, dto);
        }

        public async Task<bool> ChangePasswordAsync(int employeeId, ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword)) return false;
            if (dto.NewPassword != dto.ConfirmNewPassword) return false;

            var user = await _userRepo.GetByEmployeeIdAsync(employeeId);
            if (user == null) return false;

            if (!VerifyPassword(dto.CurrentPassword, user.Password)) return false;

            string hashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            return await _userRepo.UpdatePasswordByEmployeeIdAsync(employeeId, hashed);
        }

        /// <summary>
        /// Verifies a password against a stored BCrypt hash. A stored value that is not a
        /// valid BCrypt hash (e.g. unmigrated seed data) is treated as a mismatch instead
        /// of throwing.
        /// </summary>
        private static bool VerifyPassword(string? plainText, string storedHash)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(storedHash))
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
