namespace ETMS.Application.DTOs.Dashboard
{
    // ── Metrics shared across dashboards ──────────────────────────────────
    public class DashboardMetrics
    {
        public int ActiveRequests { get; set; }
        public int PendingApprovals { get; set; }
        public int Rejected { get; set; }
        public double AvgApprovalDays { get; set; }
        public int TotalOpenPositions { get; set; }
        public Dictionary<string, int> OpenPositionsByLocation { get; set; } = new();
    }

    // ── Used in approval queue tables ─────────────────────────────────────
    public class PendingApprovalDto
    {
        public int TransferRequestId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string FromLocation { get; set; }
        public string FromDepartment { get; set; }
        public string TargetLocation { get; set; }
        public string TargetDepartment { get; set; }
        public string TransferType { get; set; }   // e.g. "Permanent", "Temporary"
        public string Reason { get; set; }
        public DateTime RequestDate { get; set; }
        public string CurrentStatus { get; set; }  // from StatusMaster
        // Prior stage info (so approver can see trail)
        public string ManagerDecision { get; set; }
        public string HODDecision { get; set; }
    }

    // ── Already-actioned item shown in history table ──────────────────────
    public class ActionedRequestDto
    {
        public int TransferRequestId { get; set; }
        public string EmployeeName { get; set; }
        public string TargetLocation { get; set; }
        public string TargetDepartment { get; set; }
        public string Decision { get; set; }  // "Approved" | "Rejected"
        public DateTime ActionedOn { get; set; }
        public string Comments { get; set; }
        public string FinalStatus { get; set; }
        public string LetterPath { get; set; }  // populated after HR approves
    }

    // ── Open position row ─────────────────────────────────────────────────
    public class OpenPositionDto
    {
        public int PositionId { get; set; }
        public string LocationName { get; set; }
        public string DepartmentName { get; set; }
    }
}

namespace ETMS.Application.DTOs.Approval
{
    // Posted from approve/reject modal forms
    public class ApprovalActionDto
    {
        public int TransferRequestId { get; set; }
        public string Decision { get; set; }  // "Approved" | "Rejected"
        public string Comments { get; set; }
    }

    // Posted by HR/HOD to change open-position count
    public class UpdatePositionDto
    {
        public int PositionId { get; set; }
        public string LocationName { get; set; }
        public string DepartmentName { get; set; }
        public string Action { get; set; }     // "Add" | "Remove"
    }
}