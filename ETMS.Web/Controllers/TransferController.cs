using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ETMS.Web.Controllers
{
    [Authorize]
    public class TransferController : Controller
    {
        private readonly ITransferRequestRepository _transferRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public TransferController(
            ITransferRequestRepository transferRepo,
            ILocationRepository locationRepo,
            IDepartmentRepository deptRepo,
            IEmployeeRepository employeeRepo)
        {
            _transferRepo = transferRepo;
            _locationRepo = locationRepo;
            _deptRepo = deptRepo;
            _employeeRepo = employeeRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTransferViewModel();

            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(employeeIdClaim, out int employeeId))
            {
                // This calls our new helper method below to load all the left-panel data and dropdowns
                await PopulateEmployeeDisplayDataAsync(model, employeeId);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransferViewModel model)
        {
            // Ignore validation for the display-only fields on the left panel so they don't block submission
            ModelState.Remove(nameof(model.EmployeeName));
            ModelState.Remove(nameof(model.EmployeeCode));
            ModelState.Remove(nameof(model.CurrentDepartment));
            ModelState.Remove(nameof(model.CurrentLocation));
            ModelState.Remove(nameof(model.CurrentCountry));
            ModelState.Remove(nameof(model.Grade));
            ModelState.Remove(nameof(model.SBU));
            ModelState.Remove(nameof(model.CostCenter));
            ModelState.Remove(nameof(model.Company));
            ModelState.Remove(nameof(model.ImmediateSupervisor));
            ModelState.Remove(nameof(model.DepartmentHead));
            ModelState.Remove(nameof(model.HRBP));
            ModelState.Remove(nameof(model.ISName));
            ModelState.Remove(nameof(model.ISCode));
            ModelState.Remove(nameof(model.DHName));
            ModelState.Remove(nameof(model.DHCode));
            ModelState.Remove(nameof(model.HRBPName));
            ModelState.Remove(nameof(model.FromType));
            ModelState.Remove(nameof(model.Locations));
            ModelState.Remove(nameof(model.Departments));
            ModelState.Remove(nameof(model.RawLocations));

            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim))
                return RedirectToAction("Login", "Account");

            var employeeId = int.Parse(employeeIdClaim);

            // IF VALIDATION FAILS (e.g., they missed a required field)
            if (!ModelState.IsValid)
            {
                // Re-hydrate the left panel data BEFORE returning the view so the UI doesn't break into a blank screen!
                await PopulateEmployeeDisplayDataAsync(model, employeeId);
                return View(model);
            }

            var currentEmployee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

            if (currentEmployee == null)
                return RedirectToAction("Login", "Account");

            var newRequest = new TransferRequest
            {
                EmployeeId = employeeId,
                FromDepartmentId = currentEmployee.DepartmentId,
                FromLocationId = currentEmployee.LocationId,
                ToLocationId = model.ToLocationId,
                ToDepartmentId = model.ToDepartmentId,

                // Use TransferTypeAuto or default to Domestic Transfer
                TransferType = string.IsNullOrEmpty(model.TransferTypeAuto) ? "Domestic Transfer" : model.TransferTypeAuto,

                // Map Remarks to Reason since we swapped it in the UI
                Reason = model.Remarks ?? "Mobility Request",

                RequestDate = DateTime.Now,
                Status = "Pending",
                IsActive = true,

                // --- MAPPING NEW L&T FIELDS ---
                LetterType = model.LetterType,
                WithinCity = model.WithinCity,
                RelocationStatus = model.RelocationStatus,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                ProjectName = model.ProjectName,
                NewVertical = model.NewVertical,
                NewBU = model.NewBU,
                NewISPsno = model.NewISPsno,
                NewISName = model.NewISName,
                NewISEmail = model.NewISEmail,
                ICHead = model.ICHead,
                Remarks = model.Remarks
            };

            await _transferRepo.CreateAsync(newRequest);

            // Set the exact success message requested to trigger the Javascript toast
            TempData["SuccessMessage"] = "Transfer request sent for approval";
            return RedirectToAction("Index", "Dashboard");
        }

        // --- HELPER METHOD TO KEEP CODE CLEAN ---
        // This handles fetching all the complex profile data for the left panel and dropdowns
        private async Task PopulateEmployeeDisplayDataAsync(CreateTransferViewModel model, int employeeId)
        {
            var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
            if (employee != null)
            {
                model.EmployeeName = $"{employee.FirstName} {employee.LastName}";
                model.EmployeeCode = employee.EmployeeCode;

                var locations = await _locationRepo.GetAllAsync();
                var departments = await _deptRepo.GetAllAsync();

                var loc = locations.FirstOrDefault(l => l.LocationId == employee.LocationId);
                model.CurrentLocation = loc?.City ?? "Unknown";
                model.CurrentCountry = loc?.Country ?? "India";
                model.FromType = model.CurrentCountry == "India" ? "Domestic" : "International";

                var currentDept = departments.FirstOrDefault(d => d.DepartmentId == employee.DepartmentId);
                model.CurrentDepartment = currentDept?.DepartmentName ?? "Unknown";

                model.Grade = employee.Grade ?? "N/A";
                model.SBU = employee.SBU ?? "N/A";
                model.CostCenter = employee.CostCenter ?? "N/A";
                model.Company = employee.Company ?? "Larsen & Toubro Limited";
                model.HRBPName = employee.HRBP ?? "N/A";

                if (employee.ReportingManagerId.HasValue)
                {
                    var manager = await _employeeRepo.GetEmployeeByIdAsync(employee.ReportingManagerId.Value);
                    model.ISName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "Not Assigned";
                    model.ISCode = manager?.EmployeeCode ?? "N/A";
                }
                else { model.ISName = "Not Assigned"; model.ISCode = "N/A"; }

                if (currentDept != null && currentDept.HeadOfDepartmentId.HasValue)
                {
                    var deptHead = await _employeeRepo.GetEmployeeByIdAsync(currentDept.HeadOfDepartmentId.Value);
                    model.DHName = deptHead != null ? $"{deptHead.FirstName} {deptHead.LastName}" : "Not Assigned";
                    model.DHCode = deptHead?.EmployeeCode ?? "N/A";
                }
                else { model.DHName = "Not Assigned"; model.DHCode = "N/A"; }

                // Populate the dropdown options
                model.RawLocations = locations;
                model.Locations = locations.Select(l => new SelectListItem { Value = l.LocationId.ToString(), Text = l.City });
                model.Departments = departments.Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName });
            }
        }
    }
}