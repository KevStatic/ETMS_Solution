USE ETMSsol_DB;
GO

-- 1. Ensure the Designations exist so the Foreign Keys don't crash
IF NOT EXISTS (SELECT 1 FROM Designations WHERE Title = 'HR Manager')
    INSERT INTO Designations (Title, Level) VALUES ('HR Manager', 'M1');
IF NOT EXISTS (SELECT 1 FROM Designations WHERE Title = 'Site Engineer')
    INSERT INTO Designations (Title, Level) VALUES ('Site Engineer', 'L2');
IF NOT EXISTS (SELECT 1 FROM Designations WHERE Title = 'Director')
    INSERT INTO Designations (Title, Level) VALUES ('Director', 'E1');

-- Get the exact IDs dynamically
DECLARE @DesigHR INT = (SELECT TOP 1 DesignationId FROM Designations WHERE Title = 'HR Manager');
DECLARE @DesigCivil INT = (SELECT TOP 1 DesignationId FROM Designations WHERE Title = 'Site Engineer');
DECLARE @DesigDir INT = (SELECT TOP 1 DesignationId FROM Designations WHERE Title = 'Director');

DECLARE @DeptHR INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentName LIKE '%HR%' OR DepartmentName LIKE '%Human%');
DECLARE @DeptCivil INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentName LIKE '%Civil%');
DECLARE @DeptIT INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentName LIKE '%IT%' OR DepartmentName LIKE '%Tech%');

-- Fallbacks just in case Civil Dept is missing
IF @DeptCivil IS NULL BEGIN INSERT INTO Departments (DepartmentName) VALUES ('Civil Engineering'); SET @DeptCivil = SCOPE_IDENTITY(); END

-- Grabbing first available Locations and Branches for testing
DECLARE @Loc1 INT = (SELECT TOP 1 LocationId FROM Locations);
DECLARE @Branch1 INT = (SELECT TOP 1 BranchId FROM Branches);

-- 2. Insert the Employees using the dynamic IDs
IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmployeeCode = 'EMP002')
BEGIN
    INSERT INTO Employee (EmployeeCode, FirstName, LastName, DateOfJoining, EmploymentType, DepartmentId, BranchId, LocationId, DesignationId, ReportingManagerId, Status, IsActive, Grade, SBU, CostCenter, Company, HRBP)
    VALUES ('EMP002', 'Anita', 'Sharma', '2015-04-12', 'Full-Time', @DeptHR, @Branch1, @Loc1, @DesigHR, NULL, 'Active', 1, 'M2', 'Corporate HR', '10001 - HR Central', 'Larsen & Toubro Limited', 'Priya Singh');
END

IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmployeeCode = 'EMP003')
BEGIN
    INSERT INTO Employee (EmployeeCode, FirstName, LastName, DateOfJoining, EmploymentType, DepartmentId, BranchId, LocationId, DesignationId, ReportingManagerId, Status, IsActive, Grade, SBU, CostCenter, Company, HRBP)
    VALUES ('EMP003', 'Tejas', 'Patel', '2023-08-01', 'Full-Time', @DeptCivil, @Branch1, @Loc1, @DesigCivil, NULL, 'Active', 1, 'L2', 'Heavy Civil Infrastructure', '20541 - Metro Projects', 'L&T Construction', 'Anita Sharma');
END

IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmployeeCode = 'EMP004')
BEGIN
    INSERT INTO Employee (EmployeeCode, FirstName, LastName, DateOfJoining, EmploymentType, DepartmentId, BranchId, LocationId, DesignationId, ReportingManagerId, Status, IsActive, Grade, SBU, CostCenter, Company, HRBP)
    VALUES ('EMP004', 'Vikram', 'Desai', '2010-01-10', 'Full-Time', @DeptIT, @Branch1, @Loc1, @DesigDir, NULL, 'Active', 1, 'E1', 'Group Information Technology', '19000 - IT Core', 'Larsen & Toubro Limited', 'Anita Sharma');
END
GO

-- 3. Establish the Reporting Hierarchy 
DECLARE @VikramId INT = (SELECT TOP 1 EmployeeId FROM Employee WHERE EmployeeCode = 'EMP004');
IF @VikramId IS NOT NULL
BEGIN
    UPDATE Employee SET ReportingManagerId = @VikramId WHERE EmployeeCode = 'EMP001'; -- Keval reports to Vikram
    UPDATE Employee SET ReportingManagerId = @VikramId WHERE EmployeeCode = 'EMP003'; -- Tejas reports to Vikram
END
GO

-- 4. Create Login Accounts securely
IF NOT EXISTS (SELECT 1 FROM UserAccounts WHERE Username = 'anita_hr')
BEGIN
    INSERT INTO UserAccounts (EmployeeId, Username, Password, Role, IsActive)
    VALUES ((SELECT EmployeeId FROM Employee WHERE EmployeeCode = 'EMP002'), 'anita_hr', 'pass123', 'HR', 1);
END

IF NOT EXISTS (SELECT 1 FROM UserAccounts WHERE Username = 'tejas_emp')
BEGIN
    INSERT INTO UserAccounts (EmployeeId, Username, Password, Role, IsActive)
    VALUES ((SELECT EmployeeId FROM Employee WHERE EmployeeCode = 'EMP003'), 'tejas_emp', 'pass123', 'Employee', 1);
END

IF NOT EXISTS (SELECT 1 FROM UserAccounts WHERE Username = 'vikram_hod')
BEGIN
    INSERT INTO UserAccounts (EmployeeId, Username, Password, Role, IsActive)
    VALUES ((SELECT EmployeeId FROM Employee WHERE EmployeeCode = 'EMP004'), 'vikram_hod', 'pass123', 'Admin', 1);
END
GO