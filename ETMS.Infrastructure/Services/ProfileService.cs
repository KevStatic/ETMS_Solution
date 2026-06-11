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
            if (dto.NewPassword != dto.ConfirmNewPassword) return false;

            var user = await _userRepo.GetByEmployeeIdAsync(employeeId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password)) return false;

            string hashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword!);
            return await _userRepo.UpdatePasswordByEmployeeIdAsync(employeeId, hashed);
        }
    }
}
