USE ETMSsol_DB;
GO

-- Check the approved request
SELECT tr.TransferRequestId, tr.Status, tr.LetterType,
       e.FirstName, e.LastName, e.LocationId, e.DepartmentId
FROM TransferRequests tr
INNER JOIN Employee e ON e.EmployeeId = tr.EmployeeId
WHERE tr.TransferRequestId = 5;

-- Check TransferHistory got logged
SELECT * FROM TransferHistory WHERE EmployeeId = 1;