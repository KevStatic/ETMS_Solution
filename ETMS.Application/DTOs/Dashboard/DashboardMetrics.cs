namespace ETMS.Application.DTOs
{
    public class DashboardMetrics
    {
        public int ActiveRequests { get; set; }
        public int PendingApprovals { get; set; }
        public int Rejected { get; set; }
        public int AvgApprovalDays { get; set; }

        // New Open Positions properties
        public int TotalOpenPositions { get; set; }
        public List<KeyValuePair<string, int>> OpenPositionsByLocation { get; set; } = new List<KeyValuePair<string, int>>();
    }
}