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
    [Route("portal")]   // Hides "Dashboard"
    public class DashboardController : Controller
    {
        private readonly ITransferRequestRepository _transferRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public DashboardController(
            ITransferRequestRepository transferRepo,
            IEmployeeRepository employeeRepo)
        {
            _transferRepo = transferRepo;
            _employeeRepo = employeeRepo;
        }

        [HttpGet("")]   // Hides "Index"
        public async Task<IActionResult> Index(
            string searchTerm,
            string sortOrder,
            string filterType = "Active")
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

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            string currentRole = roleClaim?.Value ?? "Employee";

            // ==========================================
            // 2. FETCH DATA
            // ==========================================

            var employee = await _employeeRepo.GetEmployeeByIdAsync(currentEmployeeId);

            if (employee == null)
                return Content($"Error: Employee with ID {currentEmployeeId} not found.");

            var requests = await _transferRepo.GetByEmployeeIdAsync(currentEmployeeId);

            // ==========================================
            // 3. FILTER
            // ==========================================

            ViewBag.CurrentFilter = filterType;

            if (filterType == "Active")
                requests = requests.Where(r => r.Status == "Pending");

            // ==========================================
            // 4. SEARCH
            // ==========================================

            ViewBag.CurrentSearch = searchTerm;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();

                requests = requests.Where(r =>
                    $"tr-{r.TransferRequestId:D5}".Contains(searchTerm) ||
                    r.TransferRequestId.ToString().Contains(searchTerm) ||
                    r.RequestDate.ToString("dd MMM, yyyy").ToLower().Contains(searchTerm) ||
                    (r.Status != null && r.Status.ToLower().Contains(searchTerm)) ||
                    (r.TransferType != null && r.TransferType.ToLower().Contains(searchTerm)) ||
                    (r.ToLocation != null && r.ToLocation.ToLower().Contains(searchTerm)) ||
                    (r.ToDepartment != null && r.ToDepartment.ToLower().Contains(searchTerm))
                );
            }

            // ==========================================
            // 5. SORT
            // ==========================================

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
                default:
                    requests = requests.OrderByDescending(r => r.RequestDate);
                    break;
            }

            // ==========================================
            // 6. MAP TO VIEWMODEL
            // ==========================================

            var model = new DashboardViewModel
            {
                // Mapping view for the following fields
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                
                EmployeeCode = employee.EmployeeCode,
                
                Role = currentRole,
                
                CurrentLocation = employee.Location != null? $"{employee.Location.City}, {employee.Location.State},{employee.Location.Country}": "N/A",

                
                Department = employee.Department?.DepartmentName ?? "N/A",
                
                ManagerName = employee.ReportingManager != null? $"{employee.ReportingManager.FirstName} {employee.ReportingManager.LastName}": "Not Assigned",

                Requests = requests.Select(r => new TransferRequest
                {
                    TransferRequestId = r.TransferRequestId,
                    EmployeeId = r.EmployeeId,
                    RequestDate = r.RequestDate,
                    Status = r.Status,
                    TransferType = r.TransferType,
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