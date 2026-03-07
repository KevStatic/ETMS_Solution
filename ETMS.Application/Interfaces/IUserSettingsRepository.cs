using ETMS.Application.DTOs.Profile;

namespace ETMS.Application.Interfaces
{
    public interface IUserSettingsRepository
    {
        Task<UserSettingsDto> GetByUserIdAsync(int userId);
        Task<bool> UpdateAsync(int userId, UserSettingsDto dto);
    }
}