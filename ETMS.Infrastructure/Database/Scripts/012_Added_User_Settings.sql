USE ETMSsol_DB;
GO

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
        CONSTRAINT UQ_UserSettings_UserId UNIQUE (UserId),
        CONSTRAINT FK_UserSettings_Employee FOREIGN KEY (UserId) REFERENCES dbo.Employee(EmployeeId)
    );
END
GO
