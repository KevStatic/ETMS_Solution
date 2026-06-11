USE ETMSsol_DB;
GO

-- Replace the generic HRMS notification flags (leave/attendance/payroll) and the
-- unused privacy flags with transfer-workflow notification preferences.
-- Old columns are left in place on existing databases (harmless) but are no
-- longer read or written by the application.

IF OBJECT_ID(N'dbo.UserSettings', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.UserSettings', 'NotifyTransferStatus') IS NULL
        ALTER TABLE dbo.UserSettings ADD NotifyTransferStatus BIT NOT NULL
            CONSTRAINT DF_UserSettings_NotifyTransferStatus DEFAULT 1;

    IF COL_LENGTH('dbo.UserSettings', 'NotifyApprovalRequests') IS NULL
        ALTER TABLE dbo.UserSettings ADD NotifyApprovalRequests BIT NOT NULL
            CONSTRAINT DF_UserSettings_NotifyApprovalRequests DEFAULT 1;

    IF COL_LENGTH('dbo.UserSettings', 'NotifyLetterReady') IS NULL
        ALTER TABLE dbo.UserSettings ADD NotifyLetterReady BIT NOT NULL
            CONSTRAINT DF_UserSettings_NotifyLetterReady DEFAULT 1;
END;
GO
