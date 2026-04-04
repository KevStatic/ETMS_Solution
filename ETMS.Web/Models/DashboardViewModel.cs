using ETMS.Application.DTOs;
using ETMS.Application.DTOs.Dashboard;
using ETMS.Application.DTOs.Transfer;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETMS.Web.Models
{
    public class DashboardViewModel
    {
        public string UserLocationName { get; set; } = "";
        public string UserDepartmentName { get; set; } = "";
        public ETMS.Application.DTOs.Dashboard.DashboardMetrics Metrics { get; set; } = new();
        public List<DashboardRequestItem> Requests { get; set; } = new();
        public IEnumerable<SelectListItem> Locations { get; set; } = [];
        public IEnumerable<SelectListItem> Departments { get; set; } = [];
    }
}
