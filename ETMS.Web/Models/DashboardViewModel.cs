using ETMS.Domain.Entities;

namespace ETMS.Web.Models
{
    public class DashboardViewModel
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;

        // Lists of requests for the dashboard
        public IEnumerable<TransferRequest> Requests { get; set; } = new List<TransferRequest>();

        // Stats for the top cards
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
    }
}
