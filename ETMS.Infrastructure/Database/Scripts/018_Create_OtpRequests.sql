USE ETMSsol_DB;
GO

-- One-time-password store used by both the forgot-password flow and login 2FA.
-- Codes are stored as BCrypt hashes; rows are short-lived and marked used once spent.
IF OBJECT_ID(N'dbo.OtpRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OtpRequests
    (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OtpRequests PRIMARY KEY,
        Email       NVARCHAR(256) NULL,
        PhoneNumber NVARCHAR(20)  NULL,
        OtpCode     NVARCHAR(200) NOT NULL,            -- BCrypt hash of the code
        Channel     NVARCHAR(20)  NOT NULL,            -- 'email' or 'phone'
        IsVerified  BIT NOT NULL CONSTRAINT DF_OtpRequests_IsVerified DEFAULT 0,
        IsUsed      BIT NOT NULL CONSTRAINT DF_OtpRequests_IsUsed DEFAULT 0,
        ExpiresAt   DATETIME2 NOT NULL,
        CreatedAt   DATETIME2 NOT NULL CONSTRAINT DF_OtpRequests_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_OtpRequests_Email ON dbo.OtpRequests (Email, IsUsed);
END;
GO
