using ETMS.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace ETMS.Web.Models
{
    public class DashboardViewModel
    {
        // --- Employee Profile Details ---
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;

        // --- Dashboard Data ---
        // CRITICAL FIX: Changed from TransferRequest to DashboardRequestItem
        public IEnumerable<DashboardRequestItem> Requests { get; set; } = new List<DashboardRequestItem>();

        public DashboardMetrics Metrics { get; set; } = new DashboardMetrics();
        public string UserLocationName { get; set; } = string.Empty;
        public string UserDepartmentName { get; set; } = string.Empty;

        // --- Filter Dropdowns ---
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    }

    // --- New UI Item Class ---
    // This perfectly formats the table rows to show "Hazira, India" instead of raw DB data
    public class DashboardRequestItem
    {
        public int TransferRequestId { get; set; }
        public string TargetLocation { get; set; }
        public string TargetDepartment { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
    }
}