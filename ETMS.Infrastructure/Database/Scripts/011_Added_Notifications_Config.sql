USE ETMSsol_DB;
GO

ALTER TABLE TransferHistory 
ALTER COLUMN OldBranchId INT NULL;

ALTER TABLE TransferHistory 
ALTER COLUMN NewBranchId INT NULL;

ALTER TABLE TransferRequests ADD LetterPath NVARCHAR(500) NULL;

CREATE TABLE Notifications (
    NotificationId  INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId      INT NOT NULL,
    Title           NVARCHAR(100) NOT NULL,
    Message         NVARCHAR(500) NOT NULL,
    IsRead          BIT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),
    TransferRequestId INT NULL,
    FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId)
);