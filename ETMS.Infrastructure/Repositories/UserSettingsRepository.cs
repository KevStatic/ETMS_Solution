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
            await EnsureUserSettingsTableAsync(conn);

            var sql = @"
                SELECT
                    UserId,
                    EmailLeaveNotif,
                    EmailAttendanceNotif,
                    EmailPayrollNotif,
                    PushInAppAlerts,
                    TwoFactorEnabled,
                    ProfileVisible,
                    ShowOnlineStatus,
                    Language,
                    Timezone,
                    Theme
                FROM UserSettings
                WHERE UserId = @UserId";
            var result = await conn.QueryFirstOrDefaultAsync<UserSettingsDto>(sql, new { UserId = userId });
            return result ?? GetDefaultSettings(userId);
        }

        public async Task<bool> UpdateAsync(int userId, UserSettingsDto dto)
        {
            using var conn = _context.CreateConnection();
            await EnsureUserSettingsTableAsync(conn);

            var sql = @"
                MERGE UserSettings AS target
                USING (SELECT @UserId AS UserId) AS source
                ON target.UserId = source.UserId
                WHEN MATCHED THEN
                    UPDATE SET
                        EmailLeaveNotif      = @EmailLeaveNotif,
                        EmailAttendanceNotif = @EmailAttendanceNotif,
                        EmailPayrollNotif    = @EmailPayrollNotif,
                        PushInAppAlerts      = @PushInAppAlerts,
                        TwoFactorEnabled     = @TwoFactorEnabled,
                        ProfileVisible       = @ProfileVisible,
                        ShowOnlineStatus     = @ShowOnlineStatus,
                        Language             = @Language,
                        Timezone             = @Timezone,
                        Theme                = @Theme,
                        UpdatedAt            = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN
                    INSERT (
                        UserId,
                        EmailLeaveNotif,
                        EmailAttendanceNotif,
                        EmailPayrollNotif,
                        PushInAppAlerts,
                        TwoFactorEnabled,
                        ProfileVisible,
                        ShowOnlineStatus,
                        Language,
                        Timezone,
                        Theme
                    )
                    VALUES (
                        @UserId,
                        @EmailLeaveNotif,
                        @EmailAttendanceNotif,
                        @EmailPayrollNotif,
                        @PushInAppAlerts,
                        @TwoFactorEnabled,
                        @ProfileVisible,
                        @ShowOnlineStatus,
                        @Language,
                        @Timezone,
                        @Theme
                    );";

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

        private static UserSettingsDto GetDefaultSettings(int userId) =>
            new()
            {
                UserId = userId,
                EmailLeaveNotif = true,
                EmailAttendanceNotif = true,
                EmailPayrollNotif = true,
                PushInAppAlerts = true,
                TwoFactorEnabled = false,
                ProfileVisible = true,
                ShowOnlineStatus = true,
                Language = "English",
                Timezone = "IST",
                Theme = "Light"
            };

        private static async Task EnsureUserSettingsTableAsync(System.Data.IDbConnection conn)
        {
            const string sql = @"
                IF OBJECT_ID(N'dbo.FK_UserSettings_Employee', N'F') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.UserSettings DROP CONSTRAINT FK_UserSettings_Employee;
                END;

                IF OBJECT_ID(N'dbo.UserSettings', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.UserSettings
                    (
                        UserSettingsId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserSettings PRIMARY KEY,
                        UserId               INT NOT NULL,
                        EmailLeaveNotif      BIT NOT NULL CONSTRAINT DF_UserSettings_EmailLeaveNotif DEFAULT 1,
                        EmailAttendanceNotif BIT NOT NULL CONSTRAINT DF_UserSettings_EmailAttendanceNotif DEFAULT 1,
                        EmailPayrollNotif    BIT NOT NULL CONSTRAINT DF_UserSettings_EmailPayrollNotif DEFAULT 1,
                        PushInAppAlerts      BIT NOT NULL CONSTRAINT DF_UserSettings_PushInAppAlerts DEFAULT 1,
                        TwoFactorEnabled     BIT NOT NULL CONSTRAINT DF_UserSettings_TwoFactorEnabled DEFAULT 0,
                        ProfileVisible       BIT NOT NULL CONSTRAINT DF_UserSettings_ProfileVisible DEFAULT 1,
                        ShowOnlineStatus     BIT NOT NULL CONSTRAINT DF_UserSettings_ShowOnlineStatus DEFAULT 1,
                        Language             NVARCHAR(50) NOT NULL CONSTRAINT DF_UserSettings_Language DEFAULT N'English',
                        Timezone             NVARCHAR(50) NOT NULL CONSTRAINT DF_UserSettings_Timezone DEFAULT N'IST',
                        Theme                NVARCHAR(20) NOT NULL CONSTRAINT DF_UserSettings_Theme DEFAULT N'Light',
                        CreatedAt            DATETIME2 NOT NULL CONSTRAINT DF_UserSettings_CreatedAt DEFAULT SYSUTCDATETIME(),
                        UpdatedAt            DATETIME2 NULL,
                        CONSTRAINT UQ_UserSettings_UserId UNIQUE (UserId)
                    );
                END";

            await conn.ExecuteAsync(sql);
        }
    }
}
