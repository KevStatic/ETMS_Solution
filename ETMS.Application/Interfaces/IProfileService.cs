using ETMS.Application.DTOs.Profile;

namespace ETMS.Application.Interfaces
{
    public interface IProfileService
    {
        Task<EmployeeProfileDto> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}