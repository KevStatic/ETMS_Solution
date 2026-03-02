/*==========================================================
    0. CREATE DATABASE (if not exists)
===========================================================*/

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ETMSsol_DB')
BEGIN
    CREATE DATABASE ETMSsol_DB;
END
GO

USE ETMSsol_DB;
GO


/*==========================================================
    1. CREATE CORE MASTER TABLES
===========================================================*/

-- LOCATIONS
CREATE TABLE Locations(
    LocationId INT IDENTITY(1, 1) PRIMARY KEY,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL
);

-- DESIGNATIONS
CREATE TABLE Designations(
    DesignationId INT IDENTITY(1, 1) PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    Level NVARCHAR(50) NOT NULL
);

-- STATUS MASTER
CREATE TABLE StatusMaster(
    StatusId INT IDENTITY(1, 1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL,
    Module NVARCHAR(50) NOT NULL
);

-- BRANCHES
CREATE TABLE Branches(
    BranchId INT IDENTITY(1, 1) PRIMARY KEY,
    BranchName NVARCHAR(100) NOT NULL,
    LocationId INT NOT NULL,
    CONSTRAINT FK_Branches_Locations FOREIGN KEY (LocationId) REFERENCES Locations(LocationId)
);

-- DEPARTMENTS
CREATE TABLE Departments(
    DepartmentId INT IDENTITY(1, 1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL,
    HeadOfDepartmentId INT NULL
);


/*==========================================================
    2. EMPLOYEE TABLE
===========================================================*/

CREATE TABLE Employee(
    EmployeeId INT IDENTITY(1, 1) PRIMARY KEY,
    EmployeeCode NVARCHAR(20) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfJoining DATETIME NOT NULL,
    EmploymentType NVARCHAR(50) NOT NULL,

    DepartmentId INT NOT NULL,
    BranchId INT NOT NULL,
    LocationId INT NOT NULL,
    DesignationId INT NOT NULL,
    ReportingManagerId INT NULL,

    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Employee_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Employee_Branch FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_Employee_Location FOREIGN KEY (LocationId) REFERENCES Locations(LocationId),
    CONSTRAINT FK_Employee_Designation FOREIGN KEY (DesignationId) REFERENCES Designations(DesignationId),
    CONSTRAINT FK_Employee_Manager FOREIGN KEY (ReportingManagerId) REFERENCES Employee(EmployeeId)
);

-- LINK DEPARTMENT HEAD
ALTER TABLE Departments
ADD CONSTRAINT FK_Departments_Head FOREIGN KEY (HeadOfDepartmentId) REFERENCES Employee(EmployeeId);


/*==========================================================
    3. TRANSFER REQUESTS
===========================================================*/

CREATE TABLE TransferRequests (
    TransferRequestId INT IDENTITY(1,1) PRIMARY KEY,

    EmployeeId INT NOT NULL,

    FromDepartmentId INT NOT NULL,
    ToDepartmentId INT NOT NULL,

    FromLocationId INT NOT NULL,
    ToLocationId INT NOT NULL,

    OldManagerId INT NULL,
    NewManagerId INT NULL,

    TransferType NVARCHAR(50) NOT NULL,
    Reason NVARCHAR(MAX) NOT NULL,

    RequestDate DATETIME NOT NULL DEFAULT GETDATE(),
    ExpectedRelievingDate DATETIME NULL,
    ExpectedJoiningDate DATETIME NULL,

    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Transfer_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId),
    CONSTRAINT FK_Transfer_FromDept FOREIGN KEY (FromDepartmentId) REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Transfer_ToDept FOREIGN KEY (ToDepartmentId) REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Transfer_FromLoc FOREIGN KEY (FromLocationId) REFERENCES Locations(LocationId),
    CONSTRAINT FK_Transfer_ToLoc FOREIGN KEY (ToLocationId) REFERENCES Locations(LocationId)
);


/*==========================================================
    4. TRANSFER APPROVALS
===========================================================*/

CREATE TABLE TransferApprovals (
    ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
    TransferRequestId INT NOT NULL,
    ApproverId INT NOT NULL,
    ApproverRole NVARCHAR(50) NOT NULL,
    ApprovalStatus NVARCHAR(50) NOT NULL,
    Remarks NVARCHAR(MAX) NULL,
    ActionDate DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Approval_Request FOREIGN KEY (TransferRequestId) REFERENCES TransferRequests(TransferRequestId),
    CONSTRAINT FK_Approval_Approver FOREIGN KEY (ApproverId) REFERENCES Employee(EmployeeId)
);


/*==========================================================
    5. TRANSFER HISTORY
===========================================================*/

CREATE TABLE TransferHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    OldDepartmentId INT NOT NULL,
    NewDepartmentId INT NOT NULL,
    OldBranchId INT NOT NULL,
    NewBranchId INT NOT NULL,
    OldLocationId INT NOT NULL,
    NewLocationId INT NOT NULL,
    EffectiveDate DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_History_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId)
);


/*==========================================================
    6. USER ACCOUNTS
===========================================================*/

CREATE TABLE UserAccounts (
    UserAccountId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_UserAccounts_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId)
);


/*==========================================================
    7. MASTER SEED DATA
===========================================================*/

INSERT INTO Locations (City, State, Country) VALUES
('Mumbai', 'Maharashtra', 'India'),
('Bangalore', 'Karnataka', 'India');

INSERT INTO Departments (DepartmentName) VALUES
('IT'), ('HR'), ('Finance');

INSERT INTO Branches (BranchName, LocationId) VALUES
('Powai HQ', 1),
('Whitefield DC', 2);

INSERT INTO Designations (Title, Level) VALUES
('Software Engineer', 'L1'),
('HR Manager', 'M1');

INSERT INTO StatusMaster (StatusName, Module) VALUES
('Pending', 'Transfer'),
('Approved', 'Transfer'),
('Rejected', 'Transfer');


/*==========================================================
    8. PRIMARY SEED EMPLOYEE + ACCOUNT
===========================================================*/

INSERT INTO Employee
(EmployeeCode, FirstName, LastName, DateOfJoining, EmploymentType, DepartmentId, BranchId, LocationId, DesignationId)
VALUES
('EMP001', 'Keval', 'Shah', GETDATE(), 'Full-Time', 1, 1, 1, 1);

DECLARE @EmpId INT = SCOPE_IDENTITY();

INSERT INTO UserAccounts (EmployeeId, Username, Password, Role)
VALUES (@EmpId, 'keval', 'pass123', 'Employee');


/*==========================================================
    9. SAMPLE TRANSFER REQUESTS
===========================================================*/

INSERT INTO TransferRequests
(EmployeeId, FromDepartmentId, ToDepartmentId, FromLocationId, ToLocationId, TransferType, Reason, Status)
VALUES
(@EmpId, 1, 1, 1, 2, 'Permanent', 'Relocation for Project X', 'Pending'),
(@EmpId, 1, 1, 1, 2, 'Temporary', 'Client onsite requirement', 'Approved');

GO