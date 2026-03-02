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
                // 1. Fetch the logged-in Employee
                var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);

                if (employee != null)
                {
                    model.EmployeeName = $"{employee.FirstName} {employee.LastName}";
                    model.EmployeeCode = employee.EmployeeCode;

                    // 2. Fetch Location and Department dynamically
                    var locations = await _locationRepo.GetAllAsync();
                    var departments = await _deptRepo.GetAllAsync();

                    var loc = locations.FirstOrDefault(l => l.LocationId == employee.LocationId);
                    model.CurrentLocation = loc?.City ?? "Unknown";
                    model.CurrentCountry = loc?.Country ?? "India";
                    model.FromType = model.CurrentCountry == "India" ? "Domestic" : "International";

                    var currentDept = departments.FirstOrDefault(d => d.DepartmentId == employee.DepartmentId);
                    model.CurrentDepartment = currentDept?.DepartmentName ?? "Unknown";

                    // 3. Map the new L&T specific fields straight from the database
                    model.Grade = employee.Grade ?? "N/A";
                    model.SBU = employee.SBU ?? "N/A";
                    model.CostCenter = employee.CostCenter ?? "N/A";
                    model.Company = employee.Company ?? "Larsen & Toubro Limited";
                    model.HRBPName = employee.HRBP ?? "N/A";

                    // 4. DYNAMICALLY FETCH IMMEDIATE SUPERVISOR (IS)
                    if (employee.ReportingManagerId.HasValue)
                    {
                        var manager = await _employeeRepo.GetEmployeeByIdAsync(employee.ReportingManagerId.Value);
                        model.ISName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "Not Assigned";
                        model.ISCode = manager?.EmployeeCode ?? "N/A";
                    }
                    else { model.ISName = "Not Assigned"; model.ISCode = "N/A"; }

                    // 5. DYNAMICALLY FETCH DEPARTMENT HEAD (DH)
                    // (Assuming your Departments table has HeadOfDepartmentId mapped in the Entity)
                    if (currentDept != null && currentDept.HeadOfDepartmentId.HasValue)
                    {
                        var deptHead = await _employeeRepo.GetEmployeeByIdAsync(currentDept.HeadOfDepartmentId.Value);
                        model.DHName = deptHead != null ? $"{deptHead.FirstName} {deptHead.LastName}" : "Not Assigned";
                        model.DHCode = deptHead?.EmployeeCode ?? "N/A";
                    }
                    else { model.DHName = "Not Assigned"; model.DHCode = "N/A"; }

                    // 6. Pass Raw Locations to View for JS filtering
                    model.RawLocations = locations;
                    model.Departments = departments.Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName });
                }
            }

            await PopulateDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransferViewModel model)
        {
            // Ignore validation for the display-only fields on the left panel
            ModelState.Remove(nameof(model.EmployeeName));
            ModelState.Remove(nameof(model.EmployeeCode));
            ModelState.Remove(nameof(model.CurrentDepartment));
            ModelState.Remove(nameof(model.CurrentLocation));
            ModelState.Remove(nameof(model.Grade));
            ModelState.Remove(nameof(model.SBU));
            ModelState.Remove(nameof(model.CostCenter));
            ModelState.Remove(nameof(model.Company));
            ModelState.Remove(nameof(model.ImmediateSupervisor));
            ModelState.Remove(nameof(model.DepartmentHead));
            ModelState.Remove(nameof(model.HRBP));
            ModelState.Remove(nameof(model.Locations));
            ModelState.Remove(nameof(model.Departments));
            ModelState.Remove(nameof(model.FromType)); // Display only
            ModelState.Remove(nameof(model.ISName));

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var employeeIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim))
                return RedirectToAction("Login", "Account");

            var employeeId = int.Parse(employeeIdClaim);
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

                // Use Remarks for Reason since we swapped it in the UI
                Reason = model.Remarks,

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

            TempData["SuccessMessage"] = "Mobility Request submitted successfully!";
            return RedirectToAction("Index", "Dashboard");
        }

        private async Task PopulateDropdownsAsync(CreateTransferViewModel model)
        {
            var locations = await _locationRepo.GetAllAsync();
            var departments = await _deptRepo.GetAllAsync();

            model.Locations = locations.Select(l => new SelectListItem
            {
                Value = l.LocationId.ToString(),
                Text = l.City
            });

            model.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName
            });
        }
    }
}