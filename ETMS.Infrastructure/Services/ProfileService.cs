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

        public async Task<EmployeeProfileDto> GetProfileAsync(int userAccountId)
        {
            var profile = await _empRepo.GetProfileByUserAccountIdAsync(userAccountId);
            return profile ?? new EmployeeProfileDto();
        }

        public async Task<bool> UpdateProfileAsync(int userAccountId, UpdateProfileDto dto)
        {
            return await _empRepo.UpdateProfileAsync(userAccountId, dto);
        }

        public async Task<bool> ChangePasswordAsync(int userAccountId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword) return false;

            var user = await _userRepo.GetByUserAccountIdAsync(userAccountId);
            if (user == null) return false;

            // Plain text comparison (use BCrypt if you hash passwords)
            if (user.Password != dto.CurrentPassword) return false;

            return await _userRepo.UpdatePasswordByIdAsync(userAccountId, dto.NewPassword!);
        }
    }
}