using Dapper;
using ETMS.Application.DTOs;
using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;
using System.Collections.Generic;
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
            var metricsSql = @"
                SELECT 
                    COUNT(CASE WHEN Status = 'Pending' THEN 1 END) AS ActiveRequests,
                    COUNT(CASE WHEN Status = 'Pending' THEN 1 END) AS PendingApprovals,
                    COUNT(CASE WHEN Status = 'Rejected' THEN 1 END) AS Rejected,
                    ISNULL(AVG(CASE WHEN ta.ActionDate IS NOT NULL 
                        THEN DATEDIFF(day, tr.RequestDate, ta.ActionDate) 
                        ELSE NULL END), 0) AS AvgApprovalDays,
            
                    -- Now reads from the REAL Open Positions table
                    (SELECT COUNT(*) FROM OpenPositions) AS TotalOpenPositions
                FROM TransferRequests tr
                LEFT JOIN TransferApprovals ta ON tr.TransferRequestId = ta.TransferRequestId
                WHERE tr.EmployeeId = @EmployeeId AND tr.IsActive = 1;
            ";

            // Now reads exactly what HR puts into the database
            var locationsSql = @"
                SELECT TOP 4 LocationName AS [Key], COUNT(*) AS [Value]
                FROM OpenPositions
                GROUP BY LocationName
                ORDER BY [Value] DESC;
            ";

            using var connection = _context.CreateConnection();

            var metrics = await connection.QueryFirstOrDefaultAsync<DashboardMetrics>(metricsSql, new { EmployeeId = employeeId })
                          ?? new DashboardMetrics();

            var positionsList = await connection.QueryAsync<KeyValuePair<string, int>>(locationsSql);
            metrics.OpenPositionsByLocation = positionsList.AsList();

            return metrics;
        }
        public async Task<TransferRequest?> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM TransferRequests WHERE TransferRequestId = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<TransferRequest>(query, new { Id = id });
        }
        public async Task DeleteAsync(int id)
        {
            var query = "DELETE FROM TransferRequests WHERE TransferRequestId = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Id = id });
        }

    }
}