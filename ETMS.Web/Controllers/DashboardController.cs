using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;

namespace ETMS.Web.Controllers
{
    [Authorize]
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
            // ==========================================
            // 1. GET CURRENT USER (REAL LOGIC)
            // ==========================================
            // We set default values just in case, but [Authorize] ensures we should have data.

            int currentEmployeeId = 0;
            string currentRole = "Employee";

            // Extract the User ID from the Identity Claims (set during Login)
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
            {
                currentEmployeeId = parsedId;
            }
            else
            {
                // Safety Net: If we can't find the ID, force them back to Login
                return RedirectToAction("Login", "Account");
            }

            // Extract the Role
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim != null)
            {
                currentRole = roleClaim.Value;
            }

            // ==========================================
            // 2. FETCH DATA FROM DB
            // ==========================================

            var employee = await _employeeRepo.GetEmployeeByIdAsync(currentEmployeeId);

            if (employee == null)
            {
                return Content($"Error: Employee with ID {currentEmployeeId} not found in database.");
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
            // 4. APPLY SEARCH (Checks ID, Status, Date, Location, etc.)
            // ==========================================
            ViewBag.CurrentSearch = searchTerm;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim(); // Clean input

                requests = requests.Where(r =>
                    // 1. FIX FOR ID: Recreate "TR-00001" format so "TR-00007" search works
                    $"tr-{r.TransferRequestId:D5}".Contains(searchTerm) ||
                    r.TransferRequestId.ToString().Contains(searchTerm) ||

                    // 2. FIX FOR DATE: Convert DB Date to "10 Feb, 2026" text so search works
                    r.RequestDate.ToString("dd MMM, yyyy").ToLower().Contains(searchTerm) ||

                    // 3. Check Status (e.g., "Pending")
                    (r.Status != null && r.Status.ToLower().Contains(searchTerm)) ||

                    // 4. Check Type (e.g., "Permanent")
                    (r.TransferType != null && r.TransferType.ToLower().Contains(searchTerm)) ||

                    // 5. Check Location/Dept (Safe null checks)
                    (r.ToLocation != null && r.ToLocation.ToLower().Contains(searchTerm)) ||
                    (r.ToDepartment != null && r.ToDepartment.ToLower().Contains(searchTerm))
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
                // Mapping view for the following fields
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                
                EmployeeCode = employee.EmployeeCode,
                
                Role = currentRole,
                
                CurrentLocation = employee.Location != null? $"{employee.Location.City}, {employee.Location.State},{employee.Location.Country}": "N/A",

                
                Department = employee.Department?.DepartmentName ?? "N/A",
                
                ManagerName = employee.ReportingManager != null? $"{employee.ReportingManager.FirstName} {employee.ReportingManager.LastName}": "Not Assigned",

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