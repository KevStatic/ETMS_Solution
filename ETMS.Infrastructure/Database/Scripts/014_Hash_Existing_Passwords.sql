USE ETMSsol_DB;
GO

-- Migrate all accounts using plain-text 'pass123' to a BCrypt hash.
-- Hash was generated with BCrypt.Net-Next cost factor 12.
-- Only updates rows whose password is still the plain-text value so
-- accounts that were already updated (e.g. via forgot-password flow)
-- are not overwritten.

UPDATE UserAccounts
SET Password = '$2a$12$neFWHcPZagH487DHPf7ZBeASOAoYx2mPL77lNPN5D3arVKmKTKUOy'
WHERE Password = 'pass123';
GO
