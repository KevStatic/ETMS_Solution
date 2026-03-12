-- Add ActionDate to TransferApprovals if not already present
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('TransferApprovals') AND name = 'ActionDate'
)
BEGIN
    ALTER TABLE TransferApprovals ADD ActionDate DATETIME2 NULL;
    ALTER TABLE TransferApprovals ADD Comments   NVARCHAR(500) NULL;
END;

--     'Pending'          → just submitted, awaiting Manager
--     'ManagerApproved'  → manager approved, awaiting HOD
--     'HODApproved'      → HOD approved, awaiting HR
--     'Approved'         → HR approved (final)
--     'Rejected'         → rejected at any stage
--     'Cancelled'        → cancelled by employee

--   TransferApprovals.ApproverRole:
--     'Manager' | 'HOD' | 'HR'

--   TransferApprovals.ApprovalStatus:
--     'Pending' | 'Approved' | 'Rejected'

-- ReportingManagerId on Employee (if not present — needed for Manager queue)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Employee') AND name = 'ReportingManagerId'
)
BEGIN
    ALTER TABLE Employee ADD ReportingManagerId INT NULL;
    -- FK optional: FOREIGN KEY REFERENCES Employee(EmployeeId)
END;

-- Add ActionDate & Comments to TransferApprovals

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('TransferApprovals')
      AND name = 'ActionDate'
)
BEGIN
    ALTER TABLE TransferApprovals
        ADD ActionDate DATETIME2 NULL,
            Comments   NVARCHAR(500) NULL;
END;

-- Setting up roles:

UPDATE UserAccounts SET Role = 'Employee' WHERE Username = 'keval';
UPDATE UserAccounts SET Role = 'HR'       WHERE Username = 'anita_hr';
UPDATE UserAccounts SET Role = 'Employee' WHERE Username = 'tejas_emp';
UPDATE UserAccounts SET Role = 'HOD'      WHERE Username = 'vikram_hod';
