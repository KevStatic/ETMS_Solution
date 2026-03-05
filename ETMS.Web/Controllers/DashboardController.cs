using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Domain.Interfaces;        // ✅ ADD THIS
using ETMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
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

        public DashboardController(ITransferRequestRepository transferRepo, IEmployeeRepository employeeRepo, ILocationRepository locationRepo,
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
            int currentEmployeeId = 0;
            string currentRole = "Employee"; // Default fallback

            // We set default values just in case, but [Authorize] ensures we should have data.
            if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
                currentEmployeeId = parsedId;
            else

            // Extract the User ID from the Identity Claims (set during Login)
            // READ THE REAL ROLE FROM THE SECURE LOGIN COOKIE
            if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
            if (roleClaim != null) currentRole = roleClaim.Value;
                currentEmployeeId = parsedId;
            else
            if (employee == null) return Content("Error: Employee not found.");

            var metrics = await _transferRepo.GetDashboardMetricsAsync(currentEmployeeId);
            var allLocations = await _locationRepo.GetAllAsync();
            var allDepartments = await _deptRepo.GetAllAsync();
            // Extract the Role
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim != null)
            {
            // ==========================================
            // ROLE-BASED ROUTING
            // ==========================================
            if (currentRole == "Manager" || currentRole == "HR")
            {
                // Fetch requests waiting for this manager's approval
                var pendingApprovals = await _transferRepo.GetPendingApprovalsForManagerAsync(currentEmployeeId);
            var employee = await _employeeRepo.GetEmployeeByIdAsync(currentEmployeeId);
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
                // "Approved" requests are considered completed, so they are hidden from this view.
            else
            ViewBag.CurrentSearch = searchTerm;
                // Standard Employee Logic (My Requests)
                var myRequests = await _transferRepo.GetByEmployeeIdAsync(currentEmployeeId);
                requests = requests.Where(r =>
                var employeeModel = new DashboardViewModel
                {
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    Role = currentRole,
                    Metrics = metrics,
                    UserLocationName = locString,
                    UserDepartmentName = deptString,
                    Requests = myRequests.Select(r => new DashboardRequestItem
                    {
                        TransferRequestId = r.TransferRequestId,
                        TargetLocation = r.ToLocation ?? "Unknown",
                        TargetDepartment = r.ToDepartment ?? "Unknown",
                        RequestDate = r.RequestDate,
                        Status = r.Status
                    }).ToList()
                };
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

                return View("EmployeeDashboard", employeeModel);
            }
        }

    }
}