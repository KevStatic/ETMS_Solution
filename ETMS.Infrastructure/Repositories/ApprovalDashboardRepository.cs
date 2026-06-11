using Dapper;
using ETMS.Application.DTOs.Approval;
using ETMS.Application.DTOs.Dashboard;
using ETMS.Application.Interfaces;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public class ApprovalDashboardRepository : IApprovalDashboardRepository
    {
        private readonly DapperContext _ctx;
        public ApprovalDashboardRepository(DapperContext ctx) => _ctx = ctx;

        // ══════════════════════════════════════════════════════════════════
        // METRICS
        // ══════════════════════════════════════════════════════════════════

        public async Task<DashboardMetrics> GetManagerMetricsAsync(int managerId)
        {
            using var conn = _ctx.CreateConnection();

            const string sql = @"
                SELECT
                    COUNT(CASE WHEN e.ReportingManagerId = @ManagerId
                                   AND tr.Status = 'Pending' THEN 1 END) AS PendingApprovals,
                    COUNT(CASE WHEN ta.ApproverId = @ManagerId
                                   AND ta.ApproverRole = 'Manager'
                                   AND ta.ApprovalStatus = 'Approved' THEN 1 END) AS ActiveRequests,
                    COUNT(CASE WHEN ta.ApproverId = @ManagerId
                                   AND ta.ApproverRole = 'Manager'
                                   AND ta.ApprovalStatus = 'Rejected' THEN 1 END) AS Rejected
                FROM TransferRequests tr
                INNER JOIN Employee e ON e.EmployeeId = tr.EmployeeId
                LEFT JOIN TransferApprovals ta ON ta.TransferRequestId = tr.TransferRequestId;

                SELECT AVG(CAST(DATEDIFF(day, tr.RequestDate, ta.ActionDate) AS FLOAT)) AS AvgDays
                FROM TransferApprovals ta
                INNER JOIN TransferRequests tr ON tr.TransferRequestId = ta.TransferRequestId
                WHERE ta.ApproverId = @ManagerId AND ta.ApprovalStatus = 'Approved';

                SELECT op.LocationName, COUNT(*) AS Cnt
                FROM OpenPositions op
                GROUP BY op.LocationName;

                SELECT COUNT(*) FROM OpenPositions;";

            using var multi = await conn.QueryMultipleAsync(sql, new { ManagerId = managerId });
            var counts = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var avg = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var byLoc = (await multi.ReadAsync<dynamic>()).ToList();
            var total = await multi.ReadFirstOrDefaultAsync<int>();

            return new DashboardMetrics
            {
                PendingApprovals = (int)(counts?.PendingApprovals ?? 0),
                ActiveRequests = (int)(counts?.ActiveRequests ?? 0),
                Rejected = (int)(counts?.Rejected ?? 0),
                AvgApprovalDays = Math.Round((double)(avg?.AvgDays ?? 0), 1),
                TotalOpenPositions = total,
                OpenPositionsByLocation = byLoc.ToDictionary(
                    (dynamic x) => (string)x.LocationName,
                    (dynamic x) => (int)x.Cnt)
            };
        }

        public async Task<DashboardMetrics> GetHODMetricsAsync(int hodEmployeeId)
        {
            using var conn = _ctx.CreateConnection();
            const string sql = @"
                SELECT
                    COUNT(CASE WHEN tr.Status = 'ManagerApproved' THEN 1 END) AS PendingApprovals,
                    COUNT(CASE WHEN ta.ApproverId = @HodId AND ta.ApprovalStatus = 'Approved' THEN 1 END) AS ActiveRequests,
                    COUNT(CASE WHEN ta.ApproverId = @HodId AND ta.ApprovalStatus = 'Rejected' THEN 1 END) AS Rejected
                FROM TransferRequests tr
                LEFT JOIN TransferApprovals ta ON ta.TransferRequestId = tr.TransferRequestId
                                              AND ta.ApproverRole = 'HOD';

                SELECT AVG(CAST(DATEDIFF(day, tr.RequestDate, ta.ActionDate) AS FLOAT)) AS AvgDays
                FROM TransferApprovals ta
                INNER JOIN TransferRequests tr ON tr.TransferRequestId = ta.TransferRequestId
                WHERE ta.ApproverId = @HodId AND ta.ApprovalStatus = 'Approved';

                SELECT op.LocationName, COUNT(*) AS Cnt FROM OpenPositions op GROUP BY op.LocationName;
                SELECT COUNT(*) FROM OpenPositions;";

            using var multi = await conn.QueryMultipleAsync(sql, new { HodId = hodEmployeeId });
            var counts = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var avg = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var byLoc = (await multi.ReadAsync<dynamic>()).ToList();
            var total = await multi.ReadFirstOrDefaultAsync<int>();

            return new DashboardMetrics
            {
                PendingApprovals = (int)(counts?.PendingApprovals ?? 0),
                ActiveRequests = (int)(counts?.ActiveRequests ?? 0),
                Rejected = (int)(counts?.Rejected ?? 0),
                AvgApprovalDays = Math.Round((double)(avg?.AvgDays ?? 0), 1),
                TotalOpenPositions = total,
                OpenPositionsByLocation = byLoc.ToDictionary(
                    (dynamic x) => (string)x.LocationName,
                    (dynamic x) => (int)x.Cnt)
            };
        }

        public async Task<DashboardMetrics> GetHRMetricsAsync()
        {
            using var conn = _ctx.CreateConnection();
            const string sql = @"
                DECLARE @Now DATETIME = GETDATE();
                SELECT
                    COUNT(CASE WHEN tr.Status IN ('Pending','ManagerApproved','HODApproved') THEN 1 END) AS PendingApprovals,
                    COUNT(CASE WHEN tr.Status = 'Approved'
                               AND MONTH(tr.RequestDate)=MONTH(@Now) AND YEAR(tr.RequestDate)=YEAR(@Now) THEN 1 END) AS ActiveRequests,
                    COUNT(CASE WHEN tr.Status = 'Rejected'
                               AND MONTH(tr.RequestDate)=MONTH(@Now) AND YEAR(tr.RequestDate)=YEAR(@Now) THEN 1 END) AS Rejected
                FROM TransferRequests tr;

                SELECT AVG(CAST(DATEDIFF(day, tr.RequestDate, ta.ActionDate) AS FLOAT)) AS AvgDays
                FROM TransferApprovals ta
                INNER JOIN TransferRequests tr ON tr.TransferRequestId = ta.TransferRequestId
                WHERE ta.ApprovalStatus = 'Approved';

                SELECT op.LocationName, COUNT(*) AS Cnt FROM OpenPositions op GROUP BY op.LocationName;
                SELECT COUNT(*) FROM OpenPositions;";

            using var multi = await conn.QueryMultipleAsync(sql);
            var counts = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var avg = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var byLoc = (await multi.ReadAsync<dynamic>()).ToList();
            var total = await multi.ReadFirstOrDefaultAsync<int>();

            return new DashboardMetrics
            {
                PendingApprovals = (int)(counts?.PendingApprovals ?? 0),
                ActiveRequests = (int)(counts?.ActiveRequests ?? 0),
                Rejected = (int)(counts?.Rejected ?? 0),
                AvgApprovalDays = Math.Round((double)(avg?.AvgDays ?? 0), 1),
                TotalOpenPositions = total,
                OpenPositionsByLocation = byLoc.ToDictionary(
                    (dynamic x) => (string)x.LocationName,
                    (dynamic x) => (int)x.Cnt)
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // PENDING QUEUES
        // ══════════════════════════════════════════════════════════════════

        public async Task<IEnumerable<PendingApprovalDto>> GetPendingForManagerAsync(int managerId)
        {
            using var conn = _ctx.CreateConnection();
            // Stage 1: requests where this manager is the reporting manager of the employee
            // and no Manager-level approval exists yet (or it's still 'Pending')
            const string sql = @"
                SELECT
                    tr.TransferRequestId,
                    tr.EmployeeId,
                    CONCAT(e.FirstName,' ',e.LastName)   AS EmployeeName,
                    e.EmployeeCode,
                    lFrom.City + ', ' + lFrom.State AS FromLocation,
                    dFrom.DepartmentName                 AS FromDepartment,
                    lTo.City + ', ' + lTo.State AS TargetLocation,
                    dTo.DepartmentName                   AS TargetDepartment,
                    tr.TransferType,
                    tr.Reason,
                    tr.RequestDate,
                    tr.Status                            AS CurrentStatus
                FROM TransferRequests tr
                INNER JOIN Employee   e     ON e.EmployeeId         = tr.EmployeeId
                INNER JOIN Locations  lFrom ON lFrom.LocationId     = tr.FromLocationId
                INNER JOIN Locations  lTo   ON lTo.LocationId       = tr.ToLocationId
                INNER JOIN Departments dFrom ON dFrom.DepartmentId  = tr.FromDepartmentId
                INNER JOIN Departments dTo   ON dTo.DepartmentId    = tr.ToDepartmentId
                WHERE e.ReportingManagerId = @ManagerId
                  AND tr.Status = 'Pending'
                ORDER BY tr.RequestDate DESC;";

            return await conn.QueryAsync<PendingApprovalDto>(sql, new { ManagerId = managerId });
        }

        public async Task<IEnumerable<PendingApprovalDto>> GetPendingForHODAsync(int hodEmployeeId)
        {
            using var conn = _ctx.CreateConnection();
            // Stage 2: manager already approved, now waiting for HOD
            const string sql = @"
                SELECT
                    tr.TransferRequestId,
                    tr.EmployeeId,
                    CONCAT(e.FirstName,' ',e.LastName)  AS EmployeeName,
                    e.EmployeeCode,
                    lFrom.City + ', ' + lFrom.State     AS FromLocation,
                    dFrom.DepartmentName                AS FromDepartment,
                    lTo.City + ', ' + lTo.State         AS TargetLocation,
                    dTo.DepartmentName                  AS TargetDepartment,
                    tr.TransferType,
                    tr.Reason,
                    tr.RequestDate,
                    tr.Status                           AS CurrentStatus,
                    mgr_ta.ApprovalStatus               AS ManagerDecision
                FROM TransferRequests tr
                INNER JOIN Employee    e     ON e.EmployeeId         = tr.EmployeeId
                INNER JOIN Locations   lFrom ON lFrom.LocationId     = tr.FromLocationId
                INNER JOIN Locations   lTo   ON lTo.LocationId       = tr.ToLocationId
                INNER JOIN Departments dFrom ON dFrom.DepartmentId   = tr.FromDepartmentId
                INNER JOIN Departments dTo   ON dTo.DepartmentId     = tr.ToDepartmentId
                LEFT JOIN  TransferApprovals mgr_ta
                    ON mgr_ta.TransferRequestId = tr.TransferRequestId
                    AND mgr_ta.ApproverRole = 'Manager'
                WHERE tr.Status = 'ManagerApproved'
                ORDER BY tr.RequestDate DESC;";

            return await conn.QueryAsync<PendingApprovalDto>(sql, new { HodId = hodEmployeeId });
        }

        public async Task<IEnumerable<PendingApprovalDto>> GetPendingForHRAsync()
        {
            using var conn = _ctx.CreateConnection();
            // Stage 3: both manager and HOD approved
            const string sql = @"
                SELECT
                    tr.TransferRequestId,
                    tr.EmployeeId,
                    CONCAT(e.FirstName,' ',e.LastName)  AS EmployeeName,
                    e.EmployeeCode,
                    lFrom.City + ', ' + lFrom.State     AS FromLocation,
                    dFrom.DepartmentName                AS FromDepartment,
                    lTo.City + ', ' + lTo.State         AS TargetLocation,
                    dTo.DepartmentName                  AS TargetDepartment,
                    tr.TransferType,
                    tr.Reason,
                    tr.RequestDate,
                    tr.Status                           AS CurrentStatus,
                    mgr_ta.ApprovalStatus               AS ManagerDecision,
                    hod_ta.ApprovalStatus               AS HODDecision
                FROM TransferRequests tr
                INNER JOIN Employee    e     ON e.EmployeeId         = tr.EmployeeId
                INNER JOIN Locations   lFrom ON lFrom.LocationId     = tr.FromLocationId
                INNER JOIN Locations   lTo   ON lTo.LocationId       = tr.ToLocationId
                INNER JOIN Departments dFrom ON dFrom.DepartmentId   = tr.FromDepartmentId
                INNER JOIN Departments dTo   ON dTo.DepartmentId     = tr.ToDepartmentId
                LEFT JOIN  TransferApprovals mgr_ta
                    ON mgr_ta.TransferRequestId = tr.TransferRequestId AND mgr_ta.ApproverRole = 'Manager'
                LEFT JOIN  TransferApprovals hod_ta
                    ON hod_ta.TransferRequestId = tr.TransferRequestId AND hod_ta.ApproverRole = 'HOD'
                WHERE tr.Status = 'HODApproved'
                ORDER BY tr.RequestDate DESC;";

            return await conn.QueryAsync<PendingApprovalDto>(sql);
        }

        // ══════════════════════════════════════════════════════════════════
        // ACTIONED HISTORY
        // ══════════════════════════════════════════════════════════════════

        public async Task<IEnumerable<ActionedRequestDto>> GetActionedByManagerAsync(int managerId, int top = 20)
        {
            using var conn = _ctx.CreateConnection();
            const string sql = @"
                SELECT TOP (@Top)
                    tr.TransferRequestId,
                    CONCAT(e.FirstName,' ',e.LastName)        AS EmployeeName,
                    lTo.City + ', ' + lTo.State               AS TargetLocation,
                    dTo.DepartmentName                        AS TargetDepartment,
                    ta.ApprovalStatus                         AS Decision,
                    ta.ActionDate                             AS ActionedOn,
                    ta.Comments,
                    tr.Status                                 AS FinalStatus,
                    tr.LetterPath                             AS LetterPath
                FROM TransferApprovals ta
                INNER JOIN TransferRequests tr ON tr.TransferRequestId = ta.TransferRequestId
                INNER JOIN Employee e           ON e.EmployeeId = tr.EmployeeId
                INNER JOIN Locations lTo        ON lTo.LocationId = tr.ToLocationId
                INNER JOIN Departments dTo      ON dTo.DepartmentId = tr.ToDepartmentId
                WHERE ta.ApproverId = @ManagerId AND ta.ApproverRole = 'Manager'
                ORDER BY ta.ActionDate DESC;";
            return await conn.QueryAsync<ActionedRequestDto>(sql, new { ManagerId = managerId, Top = top });
        }

        public async Task<IEnumerable<ActionedRequestDto>> GetActionedByHODAsync(int hodId, int top = 20)
        {
            using var conn = _ctx.CreateConnection();
            const string sql = @"
                SELECT TOP (@Top)
                    tr.TransferRequestId,
                    CONCAT(e.FirstName,' ',e.LastName)   AS EmployeeName,
                    lTo.City + ', ' + lTo.State          AS TargetLocation,
                    dTo.DepartmentName                   AS TargetDepartment,
                    ta.ApprovalStatus                    AS Decision,
                    ta.ActionDate                        AS ActionedOn,
                    ta.Comments,
                    tr.Status                            AS FinalStatus
                FROM TransferApprovals ta
                INNER JOIN TransferRequests tr ON tr.TransferRequestId = ta.TransferRequestId
                INNER JOIN Employee e           ON e.EmployeeId = tr.EmployeeId
                INNER JOIN Locations lTo        ON lTo.LocationId = tr.ToLocationId
                INNER JOIN Departments dTo      ON dTo.DepartmentId = tr.ToDepartmentId
                WHERE ta.ApproverId = @HodId AND ta.ApproverRole = 'HOD'
                ORDER BY ta.ActionDate DESC;";
            return await conn.QueryAsync<ActionedRequestDto>(sql, new { HodId = hodId, Top = top });
        }

        public async Task<IEnumerable<ActionedRequestDto>> GetAllRequestsAsync(
            string searchTerm, string filterStatus, string sortOrder)
        {
            using var conn = _ctx.CreateConnection();

            string whereClause = filterStatus switch
            {
                "Pending" => "AND tr.Status IN ('Pending','ManagerApproved','HODApproved')",
                "Approved" => "AND tr.Status = 'Approved'",
                "Rejected" => "AND tr.Status = 'Rejected'",
                _ => ""
            };
            string searchClause = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (CONCAT(e.FirstName,' ',e.LastName) LIKE @Search OR e.EmployeeCode LIKE @Search OR (lTo.City + ' ' + lTo.State) LIKE @Search)";
            string orderClause = sortOrder switch
            {
                "date_asc" => "tr.RequestDate ASC",
                "name" => "e.FirstName ASC",
                "status" => "tr.Status ASC",
                "status_desc" => "tr.Status DESC",
                _ => "tr.RequestDate DESC"
            };

            string sql = $@"
                SELECT
                    tr.TransferRequestId,
                    CONCAT(e.FirstName,' ',e.LastName)   AS EmployeeName,
                    lTo.City + ', ' + lTo.State          AS TargetLocation,
                    dTo.DepartmentName                   AS TargetDepartment,
                    hr_ta.ApprovalStatus                 AS Decision,
                    hr_ta.ActionDate                   AS ActionedOn,
                    hr_ta.Comments,
                    tr.Status                          AS FinalStatus,
                    tr.LetterPath                      AS LetterPath
                FROM TransferRequests tr
                INNER JOIN Employee    e   ON e.EmployeeId    = tr.EmployeeId
                INNER JOIN Locations   lTo ON lTo.LocationId  = tr.ToLocationId
                INNER JOIN Departments dTo ON dTo.DepartmentId = tr.ToDepartmentId
                LEFT JOIN  TransferApprovals hr_ta
                    ON hr_ta.TransferRequestId = tr.TransferRequestId AND hr_ta.ApproverRole = 'HR'
                WHERE 1=1 {whereClause} {searchClause}
                ORDER BY {orderClause};";

            return await conn.QueryAsync<ActionedRequestDto>(sql,
                new { Search = $"%{searchTerm}%" });
        }

        // ══════════════════════════════════════════════════════════════════
        // OPEN POSITIONS  (OpenPositions table: PositionId, LocationName, DepartmentName)
        // ══════════════════════════════════════════════════════════════════

        public async Task<IEnumerable<OpenPositionDto>> GetAllOpenPositionsAsync()
        {
            using var conn = _ctx.CreateConnection();
            return await conn.QueryAsync<OpenPositionDto>(
                "SELECT PositionId, LocationName, DepartmentName FROM OpenPositions ORDER BY LocationName, DepartmentName;");
        }

        public async Task AddOpenPositionAsync(string locationName, string departmentName)
        {
            using var conn = _ctx.CreateConnection();
            await conn.ExecuteAsync(
                "INSERT INTO OpenPositions (LocationName, DepartmentName) VALUES (@LocationName, @DepartmentName);",
                new { LocationName = locationName, DepartmentName = departmentName });
        }

        public async Task RemoveOpenPositionAsync(int positionId)
        {
            using var conn = _ctx.CreateConnection();
            await conn.ExecuteAsync(
                "DELETE FROM OpenPositions WHERE PositionId = @PositionId;",
                new { PositionId = positionId });
        }

        public async Task<IEnumerable<ApprovalTrailDto>> GetApprovalTrailAsync(int transferRequestId)
        {
            using var conn = _ctx.CreateConnection();
            return await conn.QueryAsync<ApprovalTrailDto>(@"
        SELECT
            ta.ApproverRole,
            CONCAT(e.FirstName, ' ', e.LastName) AS ApproverName,
            ta.ApprovalStatus,
            ta.Comments,
            ta.ActionDate
        FROM TransferApprovals ta
        INNER JOIN Employee e ON e.EmployeeId = ta.ApproverId
        WHERE ta.TransferRequestId = @Id
        ORDER BY ta.ActionDate ASC;",
                new { Id = transferRequestId });
        }
    }
}
