using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public class ApprovalService : IApprovalService
    {
        private readonly DapperContext _ctx;

        private static readonly Dictionary<string, string> _nextStatus = new()
        {
            { "Manager", "ManagerApproved" },
            { "HOD",     "HODApproved"     },
            { "HR",      "Approved"        }
        };

        public ApprovalService(DapperContext ctx) => _ctx = ctx;

        public async Task<bool> ProcessApprovalAsync(
            int requestId, int approverId, string approverRole,
            string decision, string comments)
        {
            using var conn = _ctx.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                string expectedStatus = approverRole switch
                {
                    "Manager" => "Pending",
                    "HOD" => "ManagerApproved",
                    "HR" => "HODApproved",
                    _ => throw new ArgumentException("Unknown role")
                };

                var currentStatus = await conn.ExecuteScalarAsync<string>(
                    "SELECT Status FROM TransferRequests WHERE TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                if (currentStatus != expectedStatus) return false;

                await conn.ExecuteAsync(@"
                    MERGE TransferApprovals AS target
                    USING (SELECT @RequestId AS TransferRequestId,
                                  @Role AS ApproverRole) AS source
                    ON target.TransferRequestId = source.TransferRequestId
                       AND target.ApproverRole = source.ApproverRole
                    WHEN MATCHED THEN
                        UPDATE SET ApprovalStatus = @Decision,
                                   ApproverId     = @ApproverId,
                                   Comments       = @Comments,
                                   ActionDate     = GETDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (TransferRequestId, ApproverId, ApproverRole,
                                ApprovalStatus, Comments, ActionDate)
                        VALUES (@RequestId, @ApproverId, @Role,
                                @Decision, @Comments, GETDATE());",
                    new
                    {
                        RequestId = requestId,
                        ApproverId = approverId,
                        Role = approverRole,
                        Decision = decision,
                        Comments = comments
                    }, tx);

                string newStatus = decision == "Approved"
                    ? _nextStatus[approverRole]
                    : "Rejected";

                await conn.ExecuteAsync(
                    "UPDATE TransferRequests SET Status = @Status WHERE TransferRequestId = @Id;",
                    new { Status = newStatus, Id = requestId }, tx);

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<string> FinaliseAndGenerateLetterAsync(
            int requestId, int hrEmployeeId, string comments)
        {
            bool ok = await ProcessApprovalAsync(
                requestId, hrEmployeeId, "HR", "Approved", comments);
            if (!ok) return null;

            using var conn = _ctx.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var req = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT tr.*,
                           lTo.LocationName  AS TargetLocationName,
                           dTo.DepartmentName AS TargetDeptName,
                           CONCAT(e.FirstName,' ',e.LastName) AS FullName,
                           e.EmployeeCode
                    FROM TransferRequests tr
                    INNER JOIN Locations   lTo ON lTo.LocationId   = tr.ToLocationId
                    INNER JOIN Departments dTo ON dTo.DepartmentId = tr.ToDepartmentId
                    INNER JOIN Employee    e   ON e.EmployeeId     = tr.EmployeeId
                    WHERE tr.TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                // Remove one matching open position
                await conn.ExecuteAsync(@"
                    DELETE TOP(1) FROM OpenPositions
                    WHERE LocationName   = @Location
                      AND DepartmentName = @Dept;",
                    new
                    {
                        Location = (string)req.TargetLocationName,
                        Dept = (string)req.TargetDeptName
                    }, tx);

                // Store letter path
                string letterPath = $"/letters/TL_{req.EmployeeCode}_{requestId}_{DateTime.UtcNow:yyyyMMdd}.pdf";

                await conn.ExecuteAsync(
                    "UPDATE TransferRequests SET LetterType = @Path WHERE TransferRequestId = @Id;",
                    new { Path = letterPath, Id = requestId }, tx);

                // Log to TransferHistory
                await conn.ExecuteAsync(@"
                    INSERT INTO TransferHistory
                        (EmployeeId, OldDepartmentId, NewDepartmentId,
                         OldLocationId, NewLocationId, EffectiveDate)
                    SELECT tr.EmployeeId, tr.FromDepartmentId, tr.ToDepartmentId,
                           tr.FromLocationId, tr.ToLocationId, GETDATE()
                    FROM TransferRequests tr
                    WHERE tr.TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                // Update Employee record
                await conn.ExecuteAsync(@"
                    UPDATE e
                    SET e.DepartmentId = tr.ToDepartmentId,
                        e.LocationId   = tr.ToLocationId
                    FROM Employee e
                    INNER JOIN TransferRequests tr ON tr.EmployeeId = e.EmployeeId
                    WHERE tr.TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                tx.Commit();
                return letterPath;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}