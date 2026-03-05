USE ETMSsol_DB;
GO

-- 2. Insert Master Data (Location, Dept, Branch, Designation)
INSERT INTO Locations (City, State, Country) VALUES 
('Mumbai', 'Maharashtra', 'India'),       -- LocationId = 1
('Bangalore', 'Karnataka', 'India');      -- LocationId = 2

INSERT INTO Departments (DepartmentName) VALUES 
('IT'),         -- DeptId = 1
('HR'),         -- DeptId = 2
('Finance');    -- DeptId = 3

INSERT INTO Branches (BranchName, LocationId) VALUES 
('Powai HQ', 1),        -- BranchId = 1 (Mumbai)
('Whitefield DC', 2);   -- BranchId = 2 (Bangalore)

INSERT INTO Designations (Title, Level) VALUES 
('Software Engineer', 'L1'), -- DesgId = 1
('HR Manager', 'M1');        -- DesgId = 2

-- 3. Insert Employee (The Core User)
-- We explicitly set IDs if IDENTITY_INSERT is ON, but here we let SQL auto-gen.
-- Assuming table is fresh, this will be EmployeeId = 1
INSERT INTO Employee 
(EmployeeCode, FirstName, LastName, DateOfJoining, EmploymentType, DepartmentId, BranchId, LocationId, DesignationId, Status, IsActive) 
VALUES
('EMP001', 'Keval', 'Shah', GETDATE(), 'Full-Time', 1, 1, 1, 1, 'Active', 1);

-- 4. Insert User Account (Login: keval / pass123)
DECLARE @EmpId INT = (SELECT Top 1 EmployeeId FROM Employee WHERE EmployeeCode = 'EMP001');

INSERT INTO UserAccounts (EmployeeId, Username, Password, Role, IsActive) 
VALUES (@EmpId, 'keval', 'pass123', 'Employee', 1);

-- 5. Insert Transfer Requests (So the dashboard isn't empty)
-- Request 1: Pending (Mumbai -> Bangalore)
INSERT INTO TransferRequests 
(EmployeeId, FromDepartmentId, ToDepartmentId, FromLocationId, ToLocationId, TransferType, Reason, RequestDate, Status, IsActive)
VALUES 
(@EmpId, 1, 1, 1, 2, 'Permanent', 'Relocation for Project X', GETDATE(), 'Pending', 1);

-- Request 2: Approved (History)
INSERT INTO TransferRequests 
(EmployeeId, FromDepartmentId, ToDepartmentId, FromLocationId, ToLocationId, TransferType, Reason, RequestDate, Status, IsActive)
VALUES 
(@EmpId, 1, 1, 1, 2, 'Temporary', 'Onsite Visit', GETDATE()-30, 'Approved', 1);

GO