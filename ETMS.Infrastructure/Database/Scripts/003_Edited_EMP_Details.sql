USE ETMSsol_DB;
GO

-- 1. Add the missing L&T fields to the Employee table
ALTER TABLE Employee ADD 
    Grade NVARCHAR(50) NULL,
    SBU NVARCHAR(150) NULL,
    CostCenter NVARCHAR(150) NULL,
    Company NVARCHAR(150) NULL,
    HRBP NVARCHAR(150) NULL;
GO

-- 2. Update Keval (EmployeeId = 2) with specific L&T IT data
UPDATE Employee SET 
    Grade = 'M1-C',
    SBU = 'Group Business Assurance & Enablers',
    CostCenter = '19071 - Inform''n Tech Head',
    Company = 'Larsen & Toubro Limited',
    HRBP = 'Vijayan Kadavil'
WHERE EmployeeCode = 'EMP001';

-- 3. Update Tejas (EmployeeId = 4) with different Construction data
UPDATE Employee SET 
    Grade = 'L1',
    SBU = 'Heavy Civil Infrastructure',
    CostCenter = '20541 - Metro Projects',
    Company = 'L&T Construction',
    HRBP = 'Priya Singh'
WHERE EmployeeCode = 'EMP003';
GO