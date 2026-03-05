using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Domain.Interfaces;
using ETMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ETMS.Application.DTOs;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ETMS.Web.Controllers
{
    [Authorize]
    [Route("portal")]
    public class DashboardController : Controller
    {
        private readonly ITransferRequestRepository _transferRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IDepartmentRepository _deptRepo;

        public DashboardController(
            ITransferRequestRepository transferRepo,
            IEmployeeRepository employeeRepo,
            ILocationRepository locationRepo,
            IDepartmentRepository deptRepo)
        {
            _transferRepo = transferRepo;
            _employeeRepo = employeeRepo;
            _locationRepo = locationRepo;
            _deptRepo = deptRepo;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? sortOrder,
            string filterType = "Active")
        {
            // 1. Extract claims from the secure login cookie
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);

            int currentEmployeeId = 0;
            string currentRole = "Employee"; // Default fallback

            if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
                currentEmployeeId = parsedId;

            if (roleClaim != null)
                currentRole = roleClaim.Value;

            // 2. Load employee and shared data
            var employee = await _employeeRepo.GetEmployeeByIdAsync(currentEmployeeId);
            if (employee == null) return Content("Error: Employee not found.");

            var metrics = await _transferRepo.GetDashboardMetricsAsync(currentEmployeeId);
            var allLocations = await _locationRepo.GetAllAsync();
            var allDepartments = await _deptRepo.GetAllAsync();

            // 3. Resolve location/department display strings
            string locString = employee.Location != null
                ? $"{employee.Location.City}, {employee.Location.State}, {employee.Location.Country}"
                : "N/A";
            string deptString = employee.Department?.DepartmentName ?? "N/A";

            // 4. Role-based routing
            if (currentRole == "Manager" || currentRole == "HR")
            {
                var pendingApprovals = await _transferRepo.GetPendingApprovalsForManagerAsync(currentEmployeeId);

                var managerModel = new DashboardViewModel
                {
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    Role = currentRole,
                    Metrics = metrics,
                    UserLocationName = locString,
                    UserDepartmentName = deptString,
                    Requests = pendingApprovals.Select(r => new DashboardRequestItem
                    {
                        TransferRequestId = r.TransferRequestId,
                        TargetLocation = r.ToLocation ?? "Unknown",
                        TargetDepartment = r.ToDepartment ?? "Unknown",
                        RequestDate = r.RequestDate,
                        Status = r.Status
                    }).ToList()
                };

                return View("ManagerDashboard", managerModel);
            }
            else
            {
                // 5. Standard Employee Logic (My Requests)
                var myRequests = await _transferRepo.GetByEmployeeIdAsync(currentEmployeeId);

                // Apply filter
                IEnumerable<TransferRequestListItemDto> requests = myRequests;
                if (filterType == "Active")
                    requests = requests.Where(r => r.Status != "Rejected" && r.Status != "Cancelled");

                // 6. Apply search
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    requests = requests.Where(r =>
                        (r.ToLocation != null && r.ToLocation.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (r.ToDepartment != null && r.ToDepartment.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));

                ViewBag.CurrentSearch = searchTerm;

                // 7. Apply sorting
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

                var employeeModel = new DashboardViewModel
                {
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    EmployeeCode = employee.EmployeeCode,
                    Role = currentRole,
                    Metrics = metrics,
                    UserLocationName = locString,
                    UserDepartmentName = deptString,
                    CurrentLocation = locString,
                    Department = deptString,
                    ManagerName = employee.ReportingManager != null
        ? $"{employee.ReportingManager.FirstName} {employee.ReportingManager.LastName}"
        : "Not Assigned",

                    // ADD THESE TWO:
                    Locations = allLocations.Select(l => new SelectListItem
                    {
                        Value = l.LocationId.ToString(),
                        Text = $"{l.City}, {l.Country}"
                    }),
                    Departments = allDepartments.Select(d => new SelectListItem
                    {
                        Value = d.DepartmentId.ToString(),
                        Text = d.DepartmentName
                    }),
                };

                return View("EmployeeDashboard", employeeModel);
            }
        }
    }
}