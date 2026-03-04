using ETMS.Application.DTOs;
using ETMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public DashboardMetrics Metrics { get; set; } = new DashboardMetrics();
        public string UserLocationName { get; set; } = string.Empty;
        public string UserDepartmentName { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    }
}
