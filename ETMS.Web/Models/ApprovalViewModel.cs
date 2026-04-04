using ETMS.Application.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETMS.Web.Models
{
    // ── Manager ───────────────────────────────────────────────────────────
    public class ManagerDashboardViewModel
    {
        public string ManagerName { get; set; }
        public string UserLocationName { get; set; }
        public string UserDepartmentName { get; set; }
        public DashboardMetrics Metrics { get; set; } = new();

        public IEnumerable<PendingApprovalDto> PendingApprovals { get; set; } = [];
        public IEnumerable<ActionedRequestDto> RecentlyActioned { get; set; } = [];
        public IEnumerable<DashboardRequestItem> MyRequests { get; set; } = [];

        // For filter dropdowns (reuse existing SelectListItem pattern)
        public IEnumerable<SelectListItem> Locations { get; set; } = [];
        public IEnumerable<SelectListItem> Departments { get; set; } = [];
    }

    // ── HOD ───────────────────────────────────────────────────────────────
    public class HODDashboardViewModel
    {
        public string HODName { get; set; }
        public string UserLocationName { get; set; }
        public string UserDepartmentName { get; set; }
        public DashboardMetrics Metrics { get; set; } = new();

        public IEnumerable<PendingApprovalDto> PendingApprovals { get; set; } = [];
        public IEnumerable<ActionedRequestDto> RecentlyActioned { get; set; } = [];
        public IEnumerable<DashboardRequestItem> MyRequests { get; set; } = [];
        public IEnumerable<OpenPositionDto> AllOpenPositions { get; set; } = [];

        public IEnumerable<SelectListItem> Locations { get; set; } = [];
        public IEnumerable<SelectListItem> Departments { get; set; } = [];
    }

    // ── HR ────────────────────────────────────────────────────────────────
    public class HRDashboardViewModel
    {
        public string HRName { get; set; }
        public string UserLocationName { get; set; }
        public string UserDepartmentName { get; set; }
        public DashboardMetrics Metrics { get; set; } = new();

        public IEnumerable<PendingApprovalDto> PendingHRApproval { get; set; } = [];
        public IEnumerable<ActionedRequestDto> AllRequests { get; set; } = [];
        public IEnumerable<OpenPositionDto> AllOpenPositions { get; set; } = [];

        public IEnumerable<SelectListItem> Locations { get; set; } = [];
        public IEnumerable<SelectListItem> Departments { get; set; } = [];

        // Filter state echoed back to view
        public string CurrentSearch { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
    }
}
