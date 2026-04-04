using ETMS.Application.DTOs.Profile;

namespace ETMS.Application.Interfaces
{
    public interface IUserSettingsService
    {
        Task<UserSettingsDto> GetSettingsAsync(int userId);
        Task<bool> SaveSettingsAsync(int userId, UserSettingsDto dto);
    }
}