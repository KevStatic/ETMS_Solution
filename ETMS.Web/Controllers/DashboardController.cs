using System.Security.Claims;
using ETMS.Application.Interfaces;
using ETMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETMS.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ITransferRequestRepository _transferRepo;
        private readonly IApprovalDashboardRepository _approvalRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IEmployeeRepository _empRepo;

        public DashboardController(
            ITransferRequestRepository transferRepo,
            IApprovalDashboardRepository approvalRepo,
            ILocationRepository locationRepo,
            IDepartmentRepository deptRepo,
            IEmployeeRepository empRepo)
        {
            _transferRepo = transferRepo;
            _approvalRepo = approvalRepo;
            _locationRepo = locationRepo;
            _deptRepo = deptRepo;
            _empRepo = empRepo;
        }

        // ── reads employeeId + role out of the login cookie ──────────────
        private (int employeeId, string role) GetCurrentUser()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            int.TryParse(idClaim?.Value, out int empId);
            return (empId, roleClaim?.Value ?? "Employee");
        }

        // ── dropdown helpers (reused across all dashboards) ───────────────
        private async Task<List<SelectListItem>> GetLocationItemsAsync()
        {
            var locs = await _locationRepo.GetAllAsync();
            return locs.Select(l =>
                new SelectListItem($"{l.City}, {l.State}", l.LocationId.ToString())).ToList();
        }

        private async Task<List<SelectListItem>> GetDeptItemsAsync()
        {
            var depts = await _deptRepo.GetAllAsync();
            return depts.Select(d =>
                new SelectListItem(d.DepartmentName, d.DepartmentId.ToString())).ToList();
        }

        // ═════════════════════════════════════════════════════════════════
        // INDEX — single entry point, routes by role
        // ═════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index(
            string searchTerm = null,
            string sortOrder = null,
            string filterType = "Active")
        {
            var (employeeId, role) = GetCurrentUser();
            if (employeeId == 0)
                return RedirectToAction("Login", "Account");

            return role switch
            {
                "HR" => await BuildHRDashboard(employeeId, searchTerm, sortOrder, filterType),
                "HOD" => await BuildHODDashboard(employeeId),
                "Manager" => await BuildManagerDashboard(employeeId),
                _ => await BuildEmployeeDashboard(employeeId, searchTerm, sortOrder, filterType)
            };
        }

        // ─────────────────────────────────────────────────────────────────
        // EMPLOYEE
        // ─────────────────────────────────────────────────────────────────

        private async Task<IActionResult> BuildEmployeeDashboard(
    int employeeId, string searchTerm, string sortOrder, string filterType)
        {
            var emp = await _empRepo.GetByIdAsync(employeeId);
            var metrics = await _transferRepo.GetDashboardMetricsAsync(employeeId);
            var all = await _transferRepo.GetByEmployeeIdAsync(employeeId);

            // map — note ToLocation/ToDepartment (not TargetLocation/TargetDepartment)
            var items = all.Select(r => new DashboardRequestItem
            {
                TransferRequestId = r.TransferRequestId,
                TargetLocation = r.ToLocation,
                TargetDepartment = r.ToDepartment,
                RequestDate = r.RequestDate,
                Status = r.Status
            });

            var filtered = filterType == "Active"
                ? items.Where(r => r.Status != "Rejected" && r.Status != "Cancelled")
                : items;

            if (!string.IsNullOrWhiteSpace(searchTerm))
                filtered = filtered.Where(r =>
                    r.TransferRequestId.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.Status.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

            filtered = sortOrder switch
            {
                "date_asc" => filtered.OrderBy(r => r.RequestDate),
                "date_desc" => filtered.OrderByDescending(r => r.RequestDate),
                "status" => filtered.OrderBy(r => r.Status),
                _ => filtered.OrderByDescending(r => r.RequestDate)
            };

            ViewBag.CurrentFilter = filterType ?? "Active";
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.DateSort = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewBag.StatusSort = sortOrder == "status" ? "status_desc" : "status";

            var vm = new DashboardViewModel
            {
                UserLocationName = emp?.Location != null
                         ? $"{emp.Location.City}, {emp.Location.State}"
                         : "",
                UserDepartmentName = emp?.Department?.DepartmentName ?? "",
                Metrics = metrics,
                Requests = filtered.ToList(),
                Locations = await GetLocationItemsAsync(),
                Departments = await GetDeptItemsAsync()
            };

            return View("EmployeeDashboard", vm);
        }

        // ─────────────────────────────────────────────────────────────────
        // MANAGER
        // ─────────────────────────────────────────────────────────────────
        private async Task<IActionResult> BuildManagerDashboard(int managerId)
        {
            var emp = await _empRepo.GetByIdAsync(managerId);

            var vm = new ManagerDashboardViewModel
            {
                ManagerName = $"{emp?.FirstName} {emp?.LastName}".Trim(),
                UserLocationName = emp?.Location != null
                         ? $"{emp.Location.City}, {emp.Location.State}"
                         : "",
                UserDepartmentName = emp?.Department?.DepartmentName ?? "",
                Metrics = await _approvalRepo.GetManagerMetricsAsync(managerId),
                PendingApprovals = await _approvalRepo.GetPendingForManagerAsync(managerId),
                RecentlyActioned = await _approvalRepo.GetActionedByManagerAsync(managerId),
                Locations = await GetLocationItemsAsync(),
                Departments = await GetDeptItemsAsync()
            };

            return View("ManagerDashboard", vm);
        }

        // ─────────────────────────────────────────────────────────────────
        // HOD
        // ─────────────────────────────────────────────────────────────────
        private async Task<IActionResult> BuildHODDashboard(int hodEmployeeId)
        {
            var emp = await _empRepo.GetByIdAsync(hodEmployeeId);

            var vm = new HODDashboardViewModel
            {
                HODName = $"{emp?.FirstName} {emp?.LastName}".Trim(),
                UserLocationName = emp?.Location != null
                         ? $"{emp.Location.City}, {emp.Location.State}"
                         : "",
                UserDepartmentName = emp?.Department?.DepartmentName ?? "",
                Metrics = await _approvalRepo.GetHODMetricsAsync(hodEmployeeId),
                PendingApprovals = await _approvalRepo.GetPendingForHODAsync(hodEmployeeId),
                RecentlyActioned = await _approvalRepo.GetActionedByHODAsync(hodEmployeeId),
                AllOpenPositions = await _approvalRepo.GetAllOpenPositionsAsync(),
                Locations = await GetLocationItemsAsync(),
                Departments = await GetDeptItemsAsync()
            };

            return View("HODDashboard", vm);
        }

        // ─────────────────────────────────────────────────────────────────
        // HR
        // ─────────────────────────────────────────────────────────────────
        private async Task<IActionResult> BuildHRDashboard(
            int hrEmployeeId, string searchTerm, string sortOrder, string filterType)
        {
            var emp = await _empRepo.GetByIdAsync(hrEmployeeId);

            var vm = new HRDashboardViewModel
            {
                HRName = $"{emp?.FirstName} {emp?.LastName}".Trim(),
                UserLocationName = emp?.Location != null
                         ? $"{emp.Location.City}, {emp.Location.State}"
                         : "",
                UserDepartmentName = emp?.Department?.DepartmentName ?? "",
                Metrics = await _approvalRepo.GetHRMetricsAsync(),
                PendingHRApproval = await _approvalRepo.GetPendingForHRAsync(),
                AllRequests = await _approvalRepo.GetAllRequestsAsync(
                                         searchTerm, filterType, sortOrder),
                AllOpenPositions = await _approvalRepo.GetAllOpenPositionsAsync(),
                Locations = await GetLocationItemsAsync(),
                Departments = await GetDeptItemsAsync(),
                CurrentSearch = searchTerm,
                CurrentFilter = filterType ?? "All",
                CurrentSort = sortOrder
            };

            ViewBag.CurrentFilter = vm.CurrentFilter;
            ViewBag.CurrentSearch = vm.CurrentSearch;
            ViewBag.DateSort = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewBag.StatusSort = sortOrder == "status" ? "status_desc" : "status";

            return View("HRDashboard", vm);
        }
    }
}