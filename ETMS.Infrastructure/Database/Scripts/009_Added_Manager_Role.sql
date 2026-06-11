USE ETMSsol_DB;
GO

DECLARE @DeptIT INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentName LIKE '%IT%');
DECLARE @Branch1 INT = (SELECT TOP 1 BranchId FROM Branches);
DECLARE @Loc1 INT = (SELECT TOP 1 LocationId FROM Locations);
DECLARE @DesigMgr INT = (SELECT TOP 1 DesignationId FROM Designations WHERE Title = 'Director');

IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmployeeCode = 'EMP005')
BEGIN
    INSERT INTO Employee
    (EmployeeCode, FirstName, LastName, DateOfJoining, EmploymentType,
     DepartmentId, BranchId, LocationId, DesignationId,
     ReportingManagerId, Status, IsActive, Grade, SBU, CostCenter, Company, HRBP)
    VALUES
    ('EMP005', 'Rahul', 'Mehta', '2012-05-10', 'Full-Time',
     @DeptIT, @Branch1, @Loc1, @DesigMgr,
     NULL, 'Active', 1,
     'M1', 'Group IT Services', '19001 - IT Ops',
     'Larsen & Toubro Limited', 'Anita Sharma');
END
GO

USE ETMSsol_DB;
GO

DECLARE @DeptIT INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentName LIKE '%IT%');
DECLARE @Branch1 INT = (SELECT TOP 1 BranchId FROM Branches);
DECLARE @Loc1 INT = (SELECT TOP 1 LocationId FROM Locations);
DECLARE @DesigMgr INT = (SELECT TOP 1 DesignationId FROM Designations WHERE Title = 'Director');

IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmployeeCode = 'EMP005')
BEGIN
    INSERT INTO Employee
    (EmployeeCode, FirstName, LastName, DateOfJoining, EmploymentType,
     DepartmentId, BranchId, LocationId, DesignationId,
     ReportingManagerId, Status, IsActive, Grade, SBU, CostCenter, Company, HRBP)
    VALUES
    ('EMP005', 'Rahul', 'Mehta', '2012-05-10', 'Full-Time',
     @DeptIT, @Branch1, @Loc1, @DesigMgr,
     NULL, 'Active', 1,
     'M1', 'Group IT Services', '19001 - IT Ops',
     'Larsen & Toubro Limited', 'Anita Sharma');
END
GO

IF NOT EXISTS (SELECT 1 FROM UserAccounts WHERE Username = 'rahul_mgr')
BEGIN
    INSERT INTO UserAccounts (EmployeeId, Username, Password, Role, IsActive)
    VALUES
    ((SELECT EmployeeId FROM Employee WHERE EmployeeCode = 'EMP005'),
     'rahul_mgr',
     -- BCrypt hash of the demo password 'pass123'
     '$2a$12$neFWHcPZagH487DHPf7ZBeASOAoYx2mPL77lNPN5D3arVKmKTKUOy',
     'Manager',
     1);

UPDATE UserAccounts
SET Role = 'HOD'
WHERE Username = 'vikram_hod';

END
GO