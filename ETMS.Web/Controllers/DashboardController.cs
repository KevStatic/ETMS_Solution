using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Domain.Interfaces;        // ✅ ADD THIS
using ETMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;                   // ✅ ADD THIS
namespace ETMS.Web.Controllers
{
    [Authorize]
    [Route("portal")]
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

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? sortOrder,
            string filterType = "Active")
        {
            // 1. GET CURRENT USER
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null || !int.TryParse(idClaim.Value, out int currentEmployeeId))
                return RedirectToAction("Login", "Account");

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            string currentRole = roleClaim?.Value ?? "Employee";

            // 2. FETCH DATA
            var employee = await _employeeRepo.GetEmployeeByIdAsync(currentEmployeeId);
            if (employee == null)
                return Content($"Error: Employee with ID {currentEmployeeId} not found.");

            var requests = await _transferRepo.GetByEmployeeIdAsync(currentEmployeeId);

            // 3. FILTER
            ViewBag.CurrentFilter = filterType;
            if (filterType == "Active")
                requests = requests.Where(r => r.Status == "Pending");

            // 4. SEARCH
            ViewBag.CurrentSearch = searchTerm;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower().Trim();
                requests = requests.Where(r =>
                    $"tr-{r.TransferRequestId:D5}".Contains(term) ||
                    r.TransferRequestId.ToString().Contains(term) ||
                    r.RequestDate.ToString("dd MMM, yyyy").ToLower().Contains(term) ||
                    (r.Status != null && r.Status.ToLower().Contains(term)) ||
                    (r.TransferType != null && r.TransferType.ToLower().Contains(term)) ||
                    (r.ToLocation != null && r.ToLocation.ToLower().Contains(term)) ||
                    (r.ToDepartment != null && r.ToDepartment.ToLower().Contains(term))
                );
            }

            // 5. SORT
            ViewBag.DateSort = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewBag.StatusSort = sortOrder == "Status" ? "status_desc" : "Status";

            requests = sortOrder switch
            {
                "date_asc" => requests.OrderBy(r => r.RequestDate),
                "Status" => requests.OrderBy(r => r.Status),
                "status_desc" => requests.OrderByDescending(r => r.Status),
                _ => requests.OrderByDescending(r => r.RequestDate)
            };

            // 6. BUILD MODEL
            var model = new DashboardViewModel
            {
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                EmployeeCode = employee.EmployeeCode,
                Role = currentRole,
                CurrentLocation = employee.Location != null
                    ? $"{employee.Location.City}, {employee.Location.State}, {employee.Location.Country}"
                    : "N/A",
                Department = employee.Department?.DepartmentName ?? "N/A",
                ManagerName = employee.ReportingManager != null
                    ? $"{employee.ReportingManager.FirstName} {employee.ReportingManager.LastName}"
                    : "Not Assigned",
                Requests = requests.ToList()
            };

            return View("EmployeeDashboard", model);
        }
    }
}