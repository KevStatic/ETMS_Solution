-- 1. Create Database (Run this line alone first if DB doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ETMSsol_DB')
BEGIN
    CREATE DATABASE ETMSsol_DB;
END
GO

USE ETMSsol_DB;
GO

-- ********** ETMS Database Schema Creation Script **********

-- 1. Create Locations (Independent)
CREATE TABLE Locations(
	LocationId INT IDENTITY(1, 1) PRIMARY KEY,
	City NVARCHAR(100) NOT NULL,
	State NVARCHAR(100) NOT NULL,
	Country NVARCHAR(100) NOT NULL
);

-- 2. Create Designations (Independent)
CREATE TABLE Designations(
	DesignationId INT IDENTITY(1, 1) PRIMARY KEY,
	Title NVARCHAR(100) NOT NULL,
	Level NVARCHAR(50) NOT NULL -- e.g., 'L1', 'L2', 'Senior'
);

-- 3. Create Status Master (Independent)
CREATE TABLE StatusMaster(
	StatusId INT IDENTITY(1, 1) PRIMARY KEY,
	StatusName NVARCHAR(50) NOT NULL, -- 'Pending', 'Approved', 'Rejected'
	Module NVARCHAR(50) NOT NULL -- 'Transfer', 'Onboarding'
);

-- 4. Create Branches (Depends on Locations)
CREATE TABLE Branches(
	BranchId INT IDENTITY(1, 1) PRIMARY KEY,
	BranchName NVARCHAR(100) NOT NULL,
	LocationId INT NOT NULL,
	CONSTRAINT FK_Branches_Locations FOREIGN KEY (LocationId) REFERENCES Locations(LocationId)
);

-- 5. Create Departments 
CREATE TABLE Departments(
	DepartmentId INT IDENTITY(1, 1) PRIMARY KEY,
	DepartmentName NVARCHAR(100) NOT NULL,
	HeadOfDepartmentId INT NULL -- Nullable because a new dept might not have a head yet
);

-- 6. Create Employee
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
	ReportingManagerId INT NULL, -- Self-referencing
	Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
	IsActive BIT  NOT NULL DEFAULT 1,

	CONSTRAINT FK_Employee_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
	CONSTRAINT FK_Employee_Branch FOREIGN KEY (BranchId) REFERENCES Branches(BranchId),
    CONSTRAINT FK_Employee_Location FOREIGN KEY (LocationId) REFERENCES Locations(LocationId),
    CONSTRAINT FK_Employee_Designation FOREIGN KEY (DesignationId) REFERENCES Designations(DesignationId),
    CONSTRAINT FK_Employee_Manager FOREIGN KEY (ReportingManagerId) REFERENCES Employee(EmployeeId)
);

-- 7. NOW Link Department back to Employee
ALTER TABLE Departments
ADD CONSTRAINT FK_Departments_Head FOREIGN KEY (HeadOfDepartmentId) REFERENCES Employee(EmployeeId);

-- 8. Create TransferRequests (The Core Feature)
CREATE TABLE TransferRequests (
    TransferRequestId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    FromDepartmentId INT NOT NULL,
    ToDepartmentId INT NOT NULL,
    FromLocationId INT NOT NULL,
    ToLocationId INT NOT NULL,
    OldManagerId INT NULL,
    NewManagerId INT NULL,
    TransferType NVARCHAR(50) NOT NULL, -- 'Permanent', 'Temporary'
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

-- 9. Create TransferApprovals (Workflow)
CREATE TABLE TransferApprovals (
    ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
    TransferRequestId INT NOT NULL,
    ApproverId INT NOT NULL,
    ApproverRole NVARCHAR(50) NOT NULL, -- 'HR', 'Manager'
    ApprovalStatus NVARCHAR(50) NOT NULL,
    Remarks NVARCHAR(MAX) NULL,
    ActionDate DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Approval_Request FOREIGN KEY (TransferRequestId) REFERENCES TransferRequests(TransferRequestId),
    CONSTRAINT FK_Approval_Approver FOREIGN KEY (ApproverId) REFERENCES Employee(EmployeeId)
);

-- 10. Create TransferHistory (Audit Log)
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
