-- 1. Create Database (Run this line alone first if DB doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ETMSsol_DB')
BEGIN
    CREATE DATABASE ETMSsol_DB;
END
GO

USE ETMSsol_DB;
GO

-- 2. Users Table (For Login/Auth)
-- We separate Login info (Credentials) from Employee info (Profile)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL, -- In real app, store HASH not plain text
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'HR', 'Employee')),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- 3. Employees Table (Profile Details)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
CREATE TABLE Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT UNIQUE NOT NULL, -- Links to Users table
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Department NVARCHAR(50) NOT NULL, -- e.g., 'IT', 'HR', 'Finance'
    Designation NVARCHAR(50) NOT NULL, -- e.g., 'Intern', 'Manager'
    CurrentLocation NVARCHAR(50) NOT NULL, -- e.g., 'Mumbai', 'Bangalore'
    ManagerName NVARCHAR(100),
    JoinedDate DATE DEFAULT GETDATE(),
    CONSTRAINT FK_Employees_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- 4. TransferRequests Table (The Core Feature)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TransferRequests')
CREATE TABLE TransferRequests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    CurrentLocation NVARCHAR(50) NOT NULL,
    TargetLocation NVARCHAR(50) NOT NULL,
    Reason NVARCHAR(500),
    RequestDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    HrComments NVARCHAR(500),
    CONSTRAINT FK_Transfer_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);
GO

-- =============================================
-- SEED DATA (So you don't start with empty screens)
-- =============================================

-- A. Create Users (Password is 'pass123' for everyone for now)
INSERT INTO Users (Username, PasswordHash, Role) VALUES 
('keval_admin', 'pass123', 'HR'),
('tejas_emp', 'pass123', 'Employee'),
('rahul_emp', 'pass123', 'Employee');

-- B. Create Employee Profiles
-- Get IDs dynamically to avoid errors if IDs change
DECLARE @HrId INT = (SELECT UserId FROM Users WHERE Username = 'keval_admin');
DECLARE @Emp1Id INT = (SELECT UserId FROM Users WHERE Username = 'tejas_emp');
DECLARE @Emp2Id INT = (SELECT UserId FROM Users WHERE Username = 'rahul_emp');

INSERT INTO Employees (UserId, FullName, Email, Department, Designation, CurrentLocation, ManagerName) VALUES
(@HrId, 'Keval Shah', 'keval@lt.com', 'HR', 'HR Manager', 'Mumbai', 'Director X'),
(@Emp1Id, 'Tejas Patel', 'tejas@lt.com', 'IT', 'Software Intern', 'Pune', 'Keval Shah'),
(@Emp2Id, 'Rahul Verma', 'rahul@lt.com', 'Civil', 'Site Engineer', 'Delhi', 'Keval Shah');

-- C. Create Dummy Transfer Requests
DECLARE @Emp1ProfileId INT = (SELECT EmployeeId FROM Employees WHERE UserId = @Emp1Id);

INSERT INTO TransferRequests (EmployeeId, CurrentLocation, TargetLocation, Reason, Status, RequestDate) VALUES
(@Emp1ProfileId, 'Pune', 'Mumbai', 'Family relocation', 'Pending', GETDATE()),
(@Emp1ProfileId, 'Pune', 'Bangalore', 'Project requirement', 'Rejected', GETDATE()-5);

GO