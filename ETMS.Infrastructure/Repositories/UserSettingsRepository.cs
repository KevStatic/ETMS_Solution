using Dapper;
using ETMS.Application.DTOs.Profile;
using ETMS.Application.Interfaces;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
        private readonly DapperContext _context;

        public UserSettingsRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<UserSettingsDto> GetByUserIdAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            var sql = "SELECT * FROM UserSettings WHERE UserId = @UserId";
            var result = await conn.QueryFirstOrDefaultAsync<UserSettingsDto>(sql, new { UserId = userId });
            return result ?? new UserSettingsDto { UserId = userId };
        }

        public async Task<bool> UpdateAsync(int userId, UserSettingsDto dto)
        {
            using var conn = _context.CreateConnection();
            var sql = @"UPDATE UserSettings SET
                EmailLeaveNotif      = @EmailLeaveNotif,
                EmailAttendanceNotif = @EmailAttendanceNotif,
                EmailPayrollNotif    = @EmailPayrollNotif,
                PushInAppAlerts      = @PushInAppAlerts,
                TwoFactorEnabled     = @TwoFactorEnabled,
                ProfileVisible       = @ProfileVisible,
                ShowOnlineStatus     = @ShowOnlineStatus,
                Language             = @Language,
                Timezone             = @Timezone,
                Theme                = @Theme
                WHERE UserId = @UserId";

            var rows = await conn.ExecuteAsync(sql, new
            {
                dto.EmailLeaveNotif,
                dto.EmailAttendanceNotif,
                dto.EmailPayrollNotif,
                dto.PushInAppAlerts,
                dto.TwoFactorEnabled,
                dto.ProfileVisible,
                dto.ShowOnlineStatus,
                dto.Language,
                dto.Timezone,
                dto.Theme,
                UserId = userId
            });
            return rows > 0;
        }
    }
}