using Dapper;
using ETMS.Application.DTOs.Dashboard;
using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ETMS.Infrastructure.Repositories
{
    public sealed class TransferRequestRepository : ITransferRequestRepository
    {
        private readonly DapperContext _context;

        public TransferRequestRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(TransferRequest request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO TransferRequests
(
    EmployeeId, FromDepartmentId, ToDepartmentId, FromLocationId, ToLocationId,
    OldManagerId, NewManagerId, TransferType, Reason, RequestDate, 
    ExpectedRelievingDate, ExpectedJoiningDate, Status, IsActive,
    -- NEW FIELDS
    LetterType, WithinCity, RelocationStatus, StartDate, EndDate, 
    ProjectName, NewVertical, NewBU, NewISPsno, NewISName, NewISEmail, ICHead, Remarks
)
OUTPUT INSERTED.TransferRequestId
VALUES
(
    @EmployeeId, @FromDepartmentId, @ToDepartmentId, @FromLocationId, @ToLocationId,
    @OldManagerId, @NewManagerId, @TransferType, @Reason, @RequestDate, 
    @ExpectedRelievingDate, @ExpectedJoiningDate, @Status, @IsActive,
    -- NEW FIELDS
    @LetterType, @WithinCity, @RelocationStatus, @StartDate, @EndDate, 
    @ProjectName, @NewVertical, @NewBU, @NewISPsno, @NewISName, @NewISEmail, @ICHead, @Remarks
);";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
        }

        public async Task<TransferRequest?> GetByIdAsync(int transferRequestId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM TransferRequests WHERE TransferRequestId = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<TransferRequest>(new CommandDefinition(sql, new { Id = transferRequestId }, cancellationToken: cancellationToken));
        }

        public async Task<IEnumerable<TransferRequestListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    tr.TransferRequestId,
    tr.EmployeeId,
    (e.FirstName + ' ' + e.LastName) AS EmployeeName,
    dFrom.DepartmentName AS FromDepartment,
    dTo.DepartmentName AS ToDepartment,
    (lFrom.City + ', ' + lFrom.State) AS FromLocation,
    (lTo.City + ', ' + lTo.State) AS ToLocation,
    tr.RequestDate,
    tr.Status,
    tr.TransferType
FROM TransferRequests tr
INNER JOIN Employee e ON e.EmployeeId = tr.EmployeeId
INNER JOIN Departments dFrom ON dFrom.DepartmentId = tr.FromDepartmentId
INNER JOIN Departments dTo ON dTo.DepartmentId = tr.ToDepartmentId
INNER JOIN Locations lFrom ON lFrom.LocationId = tr.FromLocationId
INNER JOIN Locations lTo ON lTo.LocationId = tr.ToLocationId
ORDER BY tr.RequestDate DESC;";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TransferRequestListItemDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        }

        public async Task<IEnumerable<TransferRequestListItemDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    tr.TransferRequestId,
    tr.EmployeeId,
    (e.FirstName + ' ' + e.LastName) AS EmployeeName,
    dFrom.DepartmentName AS FromDepartment,
    dTo.DepartmentName AS ToDepartment,
    (lFrom.City + ', ' + lFrom.State) AS FromLocation,
    (lTo.City + ', ' + lTo.State) AS ToLocation,
    tr.RequestDate,
    tr.Status,
    tr.TransferType
FROM TransferRequests tr
INNER JOIN Employee e ON e.EmployeeId = tr.EmployeeId
INNER JOIN Departments dFrom ON dFrom.DepartmentId = tr.FromDepartmentId
INNER JOIN Departments dTo ON dTo.DepartmentId = tr.ToDepartmentId
INNER JOIN Locations lFrom ON lFrom.LocationId = tr.FromLocationId
INNER JOIN Locations lTo ON lTo.LocationId = tr.ToLocationId
WHERE tr.EmployeeId = @EmployeeId
ORDER BY tr.RequestDate DESC;";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TransferRequestListItemDto>(
                new CommandDefinition(sql, new { EmployeeId = employeeId }, cancellationToken: cancellationToken));
        }

        // New: fetch pending approvals for a specific manager
        public async Task<IEnumerable<TransferRequestListItemDto>> GetPendingApprovalsForManagerAsync(int managerEmployeeId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    tr.TransferRequestId,
    tr.EmployeeId,
    (e.FirstName + ' ' + e.LastName) AS EmployeeName,
    dFrom.DepartmentName AS FromDepartment,
    dTo.DepartmentName AS ToDepartment,
    (lFrom.City + ', ' + lFrom.State) AS FromLocation,
    (lTo.City + ', ' + lTo.State) AS ToLocation,
    tr.RequestDate,
    tr.Status,
    tr.TransferType
FROM TransferRequests tr
INNER JOIN Employee e ON e.EmployeeId = tr.EmployeeId
INNER JOIN Departments dFrom ON dFrom.DepartmentId = tr.FromDepartmentId
INNER JOIN Departments dTo ON dTo.DepartmentId = tr.ToDepartmentId
INNER JOIN Locations lFrom ON lFrom.LocationId = tr.FromLocationId
INNER JOIN Locations lTo ON lTo.LocationId = tr.ToLocationId
WHERE tr.NewManagerId = @ManagerId AND tr.Status = 'Pending'
ORDER BY tr.RequestDate DESC;";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TransferRequestListItemDto>(
                new CommandDefinition(sql, new { ManagerId = managerEmployeeId }, cancellationToken: cancellationToken));
        }

        public async Task UpdateStatusAsync(int transferRequestId, string status, int actionByEmployeeId, string? remarks, CancellationToken cancellationToken = default)
        {
            const string update = @"
UPDATE TransferRequests
SET Status = @Status
WHERE TransferRequestId = @TransferRequestId;";

            const string approval = @"
INSERT INTO TransferApprovals
(
    TransferRequestId,
    ApproverId,
    ApproverRole,
    ApprovalStatus,
    Remarks
)
VALUES
(
    @TransferRequestId,
    @ApproverId,
    @ApproverRole,
    @ApprovalStatus,
    @Remarks
);";

            using var connection = _context.CreateConnection();
            using var tx = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    update,
                    new { Status = status, TransferRequestId = transferRequestId },
                    transaction: tx,
                    cancellationToken: cancellationToken));

                await connection.ExecuteAsync(new CommandDefinition(
                    approval,
                    new
                    {
                        TransferRequestId = transferRequestId,
                        ApproverId = actionByEmployeeId,
                        ApproverRole = "HR/Admin/HOD",
                        ApprovalStatus = status,
                        Remarks = remarks
                    },
                    transaction: tx,
                    cancellationToken: cancellationToken));

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<TransferRequest>> GetAllPendingAsync()
        {
            const string sql = @"
SELECT *
FROM TransferRequests
WHERE Status = 'Pending';";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TransferRequest>(sql);
        }

        public async Task<int> AddAsync(TransferRequest request)
        {
            var sql = @"
INSERT INTO TransferRequests 
(
    EmployeeId, 
    ToDepartmentId, 
    ToLocationId, 
    TransferType, 
    Reason, 
    RequestDate, 
    Status, 
    ExpectedRelievingDate,
    ExpectedJoiningDate,
    IsActive
)
VALUES 
(
    @EmployeeId, 
    @ToDepartmentId, 
    @ToLocationId, 
    @TransferType, 
    @Reason, 
    @RequestDate, 
    @Status, 
    @ExpectedRelievingDate,
    @ExpectedJoiningDate,
    1
);
SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, request);
        }

        public async Task<DashboardMetrics> GetDashboardMetricsAsync(int employeeId)
        {
            const string sql = @"
                SELECT
                    COUNT(CASE WHEN tr.Status IN ('Pending', 'ManagerApproved', 'HODApproved') THEN 1 END) AS ActiveRequests,
                    COUNT(CASE WHEN tr.Status IN ('Pending', 'ManagerApproved', 'HODApproved') THEN 1 END) AS PendingApprovals,
                    COUNT(CASE WHEN tr.Status = 'Rejected' THEN 1 END) AS Rejected
                FROM TransferRequests tr
                WHERE tr.EmployeeId = @EmployeeId AND tr.IsActive = 1;

                SELECT
                    AVG(CAST(DATEDIFF(day, tr.RequestDate, latest.ActionDate) AS FLOAT)) AS AvgApprovalDays
                FROM TransferRequests tr
                CROSS APPLY
                (
                    SELECT MAX(ta.ActionDate) AS ActionDate
                    FROM TransferApprovals ta
                    WHERE ta.TransferRequestId = tr.TransferRequestId
                      AND ta.ActionDate IS NOT NULL
                ) latest
                WHERE tr.EmployeeId = @EmployeeId
                  AND tr.IsActive = 1
                  AND latest.ActionDate IS NOT NULL;

                SELECT COUNT(*) AS TotalOpenPositions
                FROM OpenPositions;

                SELECT TOP 4 LocationName AS [Key], COUNT(*) AS [Value]
                FROM OpenPositions
                GROUP BY LocationName
                ORDER BY [Value] DESC, LocationName ASC;";

            using var connection = _context.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(sql, new { EmployeeId = employeeId });

            var counts = await multi.ReadFirstOrDefaultAsync<DashboardMetrics>() ?? new DashboardMetrics();
            var avgApprovalDays = await multi.ReadFirstOrDefaultAsync<double?>();
            var totalOpenPositions = await multi.ReadFirstOrDefaultAsync<int>();
            var positionsList = await multi.ReadAsync<KeyValuePair<string, int>>();

            counts.AvgApprovalDays = avgApprovalDays ?? 0;
            counts.TotalOpenPositions = totalOpenPositions;
            counts.OpenPositionsByLocation = positionsList.ToDictionary(x => x.Key, x => x.Value);

            return counts;
        }
        public async Task<TransferRequest?> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM TransferRequests WHERE TransferRequestId = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<TransferRequest>(query, new { Id = id });
        }
        public async Task CancelAsync(int id, int employeeId, CancellationToken cancellationToken = default)
        {
            const string query = @"
UPDATE TransferRequests
SET Status = 'Cancelled',
    IsActive = 0
WHERE TransferRequestId = @Id
  AND EmployeeId = @EmployeeId
  AND Status = 'Pending';";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(
                query,
                new { Id = id, EmployeeId = employeeId },
                cancellationToken: cancellationToken));
        }

    }
}
