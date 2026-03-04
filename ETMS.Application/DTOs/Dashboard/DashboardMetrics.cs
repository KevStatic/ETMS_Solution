namespace ETMS.Application.DTOs
{
    public class DashboardMetrics
    {
        public int ActiveRequests { get; set; }
        public int PendingApprovals { get; set; }
        public int Rejected { get; set; }
        public int AvgApprovalDays { get; set; }
        public int TotalPersonnel { get; set; }
    }
}