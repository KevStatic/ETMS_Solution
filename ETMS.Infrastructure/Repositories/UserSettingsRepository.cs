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
                    PushInAppAlerts,
                    NotifyTransferStatus,
                    NotifyApprovalRequests,
                    NotifyLetterReady,
                    TwoFactorEnabled,
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
                        PushInAppAlerts        = @PushInAppAlerts,
                        NotifyTransferStatus   = @NotifyTransferStatus,
                        NotifyApprovalRequests = @NotifyApprovalRequests,
                        NotifyLetterReady      = @NotifyLetterReady,
                        TwoFactorEnabled       = @TwoFactorEnabled,
                        Language               = @Language,
                        Timezone               = @Timezone,
                        Theme                  = @Theme,
                        UpdatedAt              = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN
                    INSERT (
                        UserId,
                        PushInAppAlerts,
                        NotifyTransferStatus,
                        NotifyApprovalRequests,
                        NotifyLetterReady,
                        TwoFactorEnabled,
                        Language,
                        Timezone,
                        Theme
                    )
                    VALUES (
                        @UserId,
                        @PushInAppAlerts,
                        @NotifyTransferStatus,
                        @NotifyApprovalRequests,
                        @NotifyLetterReady,
                        @TwoFactorEnabled,
                        @Language,
                        @Timezone,
                        @Theme
                    );";

            var rows = await conn.ExecuteAsync(sql, new
            {
                dto.PushInAppAlerts,
                dto.NotifyTransferStatus,
                dto.NotifyApprovalRequests,
                dto.NotifyLetterReady,
                dto.TwoFactorEnabled,
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
                PushInAppAlerts = true,
                NotifyTransferStatus = true,
                NotifyApprovalRequests = true,
                NotifyLetterReady = true,
                TwoFactorEnabled = false,
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
                        UserSettingsId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserSettings PRIMARY KEY,
                        UserId                 INT NOT NULL,
                        PushInAppAlerts        BIT NOT NULL CONSTRAINT DF_UserSettings_PushInAppAlerts DEFAULT 1,
                        NotifyTransferStatus   BIT NOT NULL CONSTRAINT DF_UserSettings_NotifyTransferStatus DEFAULT 1,
                        NotifyApprovalRequests BIT NOT NULL CONSTRAINT DF_UserSettings_NotifyApprovalRequests DEFAULT 1,
                        NotifyLetterReady      BIT NOT NULL CONSTRAINT DF_UserSettings_NotifyLetterReady DEFAULT 1,
                        TwoFactorEnabled       BIT NOT NULL CONSTRAINT DF_UserSettings_TwoFactorEnabled DEFAULT 0,
                        Language               NVARCHAR(50) NOT NULL CONSTRAINT DF_UserSettings_Language DEFAULT N'English',
                        Timezone               NVARCHAR(50) NOT NULL CONSTRAINT DF_UserSettings_Timezone DEFAULT N'IST',
                        Theme                  NVARCHAR(20) NOT NULL CONSTRAINT DF_UserSettings_Theme DEFAULT N'Light',
                        CreatedAt              DATETIME2 NOT NULL CONSTRAINT DF_UserSettings_CreatedAt DEFAULT SYSUTCDATETIME(),
                        UpdatedAt              DATETIME2 NULL,
                        CONSTRAINT UQ_UserSettings_UserId UNIQUE (UserId)
                    );
                END;

                -- Self-heal: add the transfer-notification columns on databases created
                -- before this schema (older deployments had leave/attendance/payroll columns).
                IF COL_LENGTH('dbo.UserSettings', 'NotifyTransferStatus') IS NULL
                    ALTER TABLE dbo.UserSettings ADD NotifyTransferStatus BIT NOT NULL CONSTRAINT DF_UserSettings_NotifyTransferStatus DEFAULT 1;
                IF COL_LENGTH('dbo.UserSettings', 'NotifyApprovalRequests') IS NULL
                    ALTER TABLE dbo.UserSettings ADD NotifyApprovalRequests BIT NOT NULL CONSTRAINT DF_UserSettings_NotifyApprovalRequests DEFAULT 1;
                IF COL_LENGTH('dbo.UserSettings', 'NotifyLetterReady') IS NULL
                    ALTER TABLE dbo.UserSettings ADD NotifyLetterReady BIT NOT NULL CONSTRAINT DF_UserSettings_NotifyLetterReady DEFAULT 1;";

            await conn.ExecuteAsync(sql);
        }
    }
}
