USE ETMSsol_DB;
GO

-- Email address used to deliver login OTP codes (2FA) and, in future, notification
-- emails. Backfilled to the Username so the column is never null for existing rows.
IF COL_LENGTH('dbo.UserAccounts', 'Email') IS NULL
    ALTER TABLE dbo.UserAccounts ADD Email NVARCHAR(256) NULL;
GO

UPDATE dbo.UserAccounts
SET Email = Username
WHERE Email IS NULL OR Email = '';
GO

-- DEMO ONLY: point a 2FA test account at a real inbox so the login OTP can be
-- received during a demo. Replace the address with your own, or delete this block
-- in production. Example:
--   UPDATE dbo.UserAccounts SET Email = 'you@example.com' WHERE Username = 'keval';
