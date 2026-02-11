-- Adds simple auth accounts for cookie login with roles (Employee / HR / HOD / Admin)
-- Run after 01_InitialSchema.sql

CREATE TABLE UserAccounts (
    UserAccountId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_UserAccounts_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId)
);

-- Example seed (adjust EmployeeId values to match your data)
-- INSERT INTO UserAccounts (EmployeeId, Username, Password, Role)
-- VALUES (1, 'emp001', 'Emp@12345', 'Employee');
-- INSERT INTO UserAccounts (EmployeeId, Username, Password, Role)
-- VALUES (2, 'hr001', 'Hr@12345', 'HR');
-- INSERT INTO UserAccounts (EmployeeId, Username, Password, Role)
-- VALUES (3, 'admin', 'Admin@12345', 'Admin');

