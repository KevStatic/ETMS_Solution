using ETMS.Application.DTOs.Profile;
using ETMS.Application.Interfaces;

namespace ETMS.Application.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserSettingsRepository _repo;

        public UserSettingsService(IUserSettingsRepository repo)
        {
            _repo = repo;
        }

        public Task<UserSettingsDto> GetSettingsAsync(int userId) =>
            _repo.GetByUserIdAsync(userId);

        public Task<bool> SaveSettingsAsync(int userId, UserSettingsDto dto) =>
            _repo.UpdateAsync(userId, dto);
    }
}