using ETMS.Application.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETMS.Web.Models
{
    public class DashboardViewModel
    {
        public string UserLocationName { get; set; } = "";
        public string UserDepartmentName { get; set; } = "";
        public DashboardMetrics Metrics { get; set; } = new();
        public List<DashboardRequestItem> Requests { get; set; } = new();
        public IEnumerable<SelectListItem> Locations { get; set; } = [];
        public IEnumerable<SelectListItem> Departments { get; set; } = [];
    }
}
