namespace ETMS.Application.DTOs.Legacy
{
    // Legacy placeholder kept only to preserve file history.
    // Use ETMS.Application.DTOs.Dashboard.DashboardMetrics in active code.
    public class LegacyDashboardMetrics
    {
        public int ActiveRequests { get; set; }
        public int PendingApprovals { get; set; }
        public int Rejected { get; set; }
        public int AvgApprovalDays { get; set; }
        public int TotalOpenPositions { get; set; }
        public List<KeyValuePair<string, int>> OpenPositionsByLocation { get; set; } = new();
    }
}
