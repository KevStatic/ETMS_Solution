using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ETMS.Web.Controllers
{
    // [Authorize] // Uncomment this once Login page finishes!
    public class DashboardController : Controller
    {
        private readonly ITransferRequestRepository _transferRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public DashboardController(ITransferRequestRepository transferRepo, IEmployeeRepository employeeRepo)
        {
            _transferRepo = transferRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string filterType = "Active")
        {
            // 1. Get Current User (Hardcoded ID until Login is ready)
            int currentEmployeeId = 1;
            string currentRole = "Employee";

            // 2. Fetch Data from DB
            var employee = await _employeeRepo.GetEmployeeByIdAsync(currentEmployeeId);

            // STOP: If DB is empty, show error (No more Developer Mock Data)
            if (employee == null)
            {
                return Content("Error: Employee not found. Please run the Seed Script to populate the database.");
            }

            var requests = await _transferRepo.GetByEmployeeIdAsync(currentEmployeeId);

            // ==========================================
            // 3. APPLY FILTERS (Active vs All)
            // ==========================================
            // Save current filter to ViewBag so the View knows which tab to highlight
            ViewBag.CurrentFilter = filterType;

            if (filterType == "Active")
            {
                // ✅ FIXED: "Active" now strictly means "Pending". 
                // "Approved" requests are considered completed, so they are hidden from this view.
                requests = requests.Where(r => r.Status == "Pending");
            }

            // ==========================================
            // 4. APPLY SEARCH
            // ==========================================
            ViewBag.CurrentSearch = searchTerm;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                requests = requests.Where(r =>
                    r.TransferRequestId.ToString().Contains(searchTerm) ||
                    r.Status.ToLower().Contains(searchTerm) ||
                    r.TransferType.ToLower().Contains(searchTerm) ||
                    // Search safe checks for potential nulls in DTO strings just in case
                    (r.EmployeeName != null && r.EmployeeName.ToLower().Contains(searchTerm)) ||
                    (r.FromDepartment != null && r.FromDepartment.ToLower().Contains(searchTerm)) ||
                    (r.ToDepartment != null && r.ToDepartment.ToLower().Contains(searchTerm)) ||
                    (r.FromLocation != null && r.FromLocation.ToLower().Contains(searchTerm)) ||
                    (r.ToLocation != null && r.ToLocation.ToLower().Contains(searchTerm)) ||
                    r.RequestDate.ToString().ToLower().Contains(searchTerm)
                );
            }

            // ==========================================
            // 5. APPLY SORTING
            // ==========================================
            // Toggle logic: If clicking Date, switch between Asc/Desc
            ViewBag.DateSort = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewBag.StatusSort = sortOrder == "Status" ? "status_desc" : "Status";

            switch (sortOrder)
            {
                case "date_asc":
                    requests = requests.OrderBy(r => r.RequestDate);
                    break;
                case "Status":
                    requests = requests.OrderBy(r => r.Status);
                    break;
                case "status_desc":
                    requests = requests.OrderByDescending(r => r.Status);
                    break;
                default: // Default: Newest First
                    requests = requests.OrderByDescending(r => r.RequestDate);
                    break;
            }

            // 6. Map to ViewModel
            var model = new DashboardViewModel
            {
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                EmployeeCode = employee.EmployeeCode,
                Role = currentRole,
                CurrentLocation = "Mumbai Branch",
                Department = "IT Department",
                ManagerName = "Sattvik Gurav",

                // MAPPING DTO -> ENTITY (To match your current ViewModel definition)
                Requests = requests.Select(r => new TransferRequest
                {
                    TransferRequestId = r.TransferRequestId,
                    EmployeeId = r.EmployeeId,
                    RequestDate = r.RequestDate,
                    Status = r.Status,
                    TransferType = r.TransferType,
                    // Fill dummy data for fields not in DTO but required by Entity object
                    Reason = string.Empty,
                    FromDepartmentId = 0,
                    ToDepartmentId = 0,
                    FromLocationId = 0,
                    ToLocationId = 0,
                    IsActive = r.Status != "Rejected" && r.Status != "Cancelled"
                }).ToList()
            };

            return View("EmployeeDashboard", model);
        }
    }
}