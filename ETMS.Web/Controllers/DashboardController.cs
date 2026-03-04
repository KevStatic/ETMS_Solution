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
            // ==========================================
            // 1. GET CURRENT USER (REAL LOGIC)
            // ==========================================
            int currentEmployeeId = 0;
            string currentRole = "Employee";

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
            {
                currentEmployeeId = parsedId;
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

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

            // Fetch new Dashboard Metrics and Master Data for Dropdowns
            var metrics = await _transferRepo.GetDashboardMetricsAsync(currentEmployeeId);
            var allLocations = await _locationRepo.GetAllAsync();
            var allDepartments = await _deptRepo.GetAllAsync();

            // ==========================================
            // 3. APPLY FILTERS (Active vs All)
            // ==========================================
            ViewBag.CurrentFilter = filterType;

            if (filterType == "Active")
            {
                requests = requests.Where(r => r.Status == "Pending");
            }

            // ==========================================
            // 4. APPLY SEARCH 
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
            // 5. APPLY SORTING
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
            string locString = employee.Location != null ? $"{employee.Location.City}, {employee.Location.State}, {employee.Location.Country}" : "N/A";
            string deptString = employee.Department?.DepartmentName ?? "N/A";

            var model = new DashboardViewModel
            {
                // Original mappings
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                EmployeeCode = employee.EmployeeCode,
                Role = currentRole,
                CurrentLocation = locString,
                Department = deptString,
                ManagerName = employee.ReportingManager != null ? $"{employee.ReportingManager.FirstName} {employee.ReportingManager.LastName}" : "Not Assigned",

                // New mappings for the UI cards, header, and filter dropdowns
                Metrics = metrics,
                UserLocationName = locString,
                UserDepartmentName = deptString,
                Locations = allLocations.Select(l => new SelectListItem { Value = l.LocationId.ToString(), Text = l.City }),
                Departments = allDepartments.Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName }),

                // MAPPING DTO -> ENTITY
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