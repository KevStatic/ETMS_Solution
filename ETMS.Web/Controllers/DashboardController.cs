using ETMS.Application.DTOs.Transfer;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using ETMS.Application.DTOs;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ETMS.Web.Controllers
{
    [Authorize]
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

        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string filterType = "Active")
        {
            int currentEmployeeId = 0;
            string currentRole = "Employee"; // Default fallback

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
                currentEmployeeId = parsedId;
            else
                return RedirectToAction("Login", "Account");

            // READ THE REAL ROLE FROM THE SECURE LOGIN COOKIE
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim != null) currentRole = roleClaim.Value;

            var employee = await _employeeRepo.GetEmployeeByIdAsync(currentEmployeeId);
            if (employee == null) return Content("Error: Employee not found.");

            var metrics = await _transferRepo.GetDashboardMetricsAsync(currentEmployeeId);
            var allLocations = await _locationRepo.GetAllAsync();
            var allDepartments = await _deptRepo.GetAllAsync();

            string locString = employee.Location != null ? $"{employee.Location.City}, {employee.Location.State}, {employee.Location.Country}" : "N/A";
            string deptString = employee.Department?.DepartmentName ?? "N/A";

            // ==========================================
            // ROLE-BASED ROUTING
            // ==========================================
            if (currentRole == "Manager" || currentRole == "HR")
            {
                // Fetch requests waiting for this manager's approval
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
                // Standard Employee Logic (My Requests)
                var myRequests = await _transferRepo.GetByEmployeeIdAsync(currentEmployeeId);

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

                return View("EmployeeDashboard", employeeModel);
            }
        }

    }
}