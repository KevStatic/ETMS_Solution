using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Web.Controllers;
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
        private readonly IUrlEncryptionService _enc;
        private readonly IApprovalDashboardRepository _approvalRepo;

        public TransferController(
            ITransferRequestRepository transferRepo,
            ILocationRepository locationRepo,
            IDepartmentRepository deptRepo,
            IEmployeeRepository employeeRepo,
            IUrlEncryptionService enc,
            IApprovalDashboardRepository approvalRepo)
        {
            _transferRepo = transferRepo;
            _locationRepo = locationRepo;
            _deptRepo = deptRepo;
            _employeeRepo = employeeRepo;
            _enc = enc;
            _approvalRepo = approvalRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? copyFrom = null)
        {
            var model = new CreateTransferViewModel();

            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";

            if (int.TryParse(employeeIdClaim, out int employeeId))
            {
                await PopulateEmployeeDisplayDataAsync(model, employeeId);

                if (!string.IsNullOrWhiteSpace(copyFrom))
                {
                    var sourceRequestId = _enc.Decrypt(copyFrom);
                    if (sourceRequestId != -1)
                    {
                        var sourceRequest = await _transferRepo.GetByIdAsync(sourceRequestId);
                        if (sourceRequest != null && sourceRequest.EmployeeId == employeeId)
                        {
                            ApplyExistingRequestToModel(model, sourceRequest);
                        }
                    }
                }
            }

            model.RequesterRoleLabel = role;

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

            // Create the new entity
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
                ExpectedRelievingDate = model.ExpectedRelievingDate,
                ExpectedJoiningDate = model.ExpectedJoiningDate,
                LetterType = model.TransferType,
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
                Remarks = model.Remarks,

                // Relocation & Reimbursement
                TravelMode = model.TravelMode,
                TravelClass = model.TravelClass,
                RelocationAllowance = model.RelocationAllowance,
                AccommodationRequired = model.AccommodationRequired,
                DependentsCount = model.DependentsCount,

                // Handover & Transition
                NoticePeriodWeeks = model.NoticePeriodWeeks,
                CurrentTaskStatus = model.CurrentTaskStatus,
                HandoverPlan = model.HandoverPlan,
                KnowledgeTransferReqd = model.KnowledgeTransferReqd
            };

            await _transferRepo.CreateAsync(newRequest);

            // Set the exact success message requested to trigger the Javascript toast
            TempData["SuccessMessage"] = "Transfer request sent for approval";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> View(string id)
        {
            var realId = _enc.Decrypt(id);
            if (realId == -1) return BadRequest("Invalid or tampered request.");

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
            int.TryParse(idClaim, out int currentEmpId);

            var request = await _transferRepo.GetByIdAsync(realId);
            if (request == null) return NotFound("Request not found.");

            // Access control per role
            if (roleClaim == "Employee")
            {
                // Employee can only see their own
                if (request.EmployeeId != currentEmpId)
                    return NotFound("Request not found or access denied.");
            }
            else if (roleClaim == "Manager")
            {
                // Manager can only see direct reports' requests
                var employee = await _employeeRepo.GetEmployeeByIdAsync(request.EmployeeId);
                if (employee == null || employee.ReportingManagerId != currentEmpId)
                    return NotFound("Request not found or access denied.");
            }
            // HOD and HR can see all requests — no extra check needed

            // Fetch location and department names
            var locations = await _locationRepo.GetAllAsync();
            var departments = await _deptRepo.GetAllAsync();

            ViewBag.FromLocation = locations.FirstOrDefault(l => l.LocationId == request.FromLocationId)
                                     is var fl && fl != null ? $"{fl.City}, {fl.State}" : "—";
            ViewBag.ToLocation = locations.FirstOrDefault(l => l.LocationId == request.ToLocationId)
                                     is var tl && tl != null ? $"{tl.City}, {tl.State}" : "—";
            ViewBag.FromDepartment = departments.FirstOrDefault(d => d.DepartmentId == request.FromDepartmentId)?.DepartmentName ?? "—";
            ViewBag.ToDepartment = departments.FirstOrDefault(d => d.DepartmentId == request.ToDepartmentId)?.DepartmentName ?? "—";
            ViewBag.UserRole = roleClaim;
            ViewBag.CanResubmit = request.EmployeeId == currentEmpId && (request.Status == "Rejected" || request.Status == "Cancelled");
            ViewBag.CanDownloadLetter = request.Status == "Approved";

            // Fetch approval trail
            ViewBag.ApprovalTrail = await _approvalRepo.GetApprovalTrailAsync(realId);

            return View("ViewTransfer", request);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var realId = _enc.Decrypt(id);
            if (realId == -1) return BadRequest("Invalid or tampered request.");

            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(employeeIdClaim, out int empId);

            var request = await _transferRepo.GetByIdAsync(realId);
            if (request == null || request.EmployeeId != empId)
                return NotFound("Request not found or access denied.");

            if (request.Status != "Pending")
                return BadRequest("Only pending transfer requests can be cancelled.");

            await _transferRepo.CancelAsync(realId, empId);
            TempData["SuccessMessage"] = "Transfer request cancelled.";
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
                model.RequesterRoleLabel = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
                model.SuggestedOpenPositions = (await _approvalRepo.GetAllOpenPositionsAsync())
                    .GroupBy(p => new { p.LocationName, p.DepartmentName })
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key.LocationName)
                    .ThenBy(g => g.Key.DepartmentName)
                    .Select(g => new OpenPositionSuggestionItem
                    {
                        LocationName = g.Key.LocationName,
                        DepartmentName = g.Key.DepartmentName,
                        OpenSlotCount = g.Count()
                    })
                    .ToList();
            }
        }

        private static void ApplyExistingRequestToModel(CreateTransferViewModel model, TransferRequest request)
        {
            model.ToLocationId = request.ToLocationId;
            model.ToDepartmentId = request.ToDepartmentId;
            model.ExpectedRelievingDate = request.ExpectedRelievingDate;
            model.ExpectedJoiningDate = request.ExpectedJoiningDate;
            model.TransferType = request.LetterType ?? request.TransferType;
            model.TransferTypeAuto = request.TransferType;
            model.WithinCity = string.IsNullOrWhiteSpace(request.WithinCity) ? model.WithinCity : request.WithinCity;
            model.RelocationStatus = string.IsNullOrWhiteSpace(request.RelocationStatus) ? model.RelocationStatus : request.RelocationStatus;
            model.StartDate = request.StartDate;
            model.EndDate = request.EndDate;
            model.ProjectName = request.ProjectName ?? string.Empty;
            model.NewVertical = request.NewVertical ?? string.Empty;
            model.NewBU = request.NewBU ?? string.Empty;
            model.NewISPsno = request.NewISPsno ?? string.Empty;
            model.NewISName = request.NewISName ?? string.Empty;
            model.NewISEmail = request.NewISEmail ?? string.Empty;
            model.ICHead = request.ICHead ?? string.Empty;
            model.Remarks = request.Remarks ?? request.Reason;

            model.TravelMode = request.TravelMode ?? string.Empty;
            model.TravelClass = request.TravelClass ?? string.Empty;
            model.RelocationAllowance = request.RelocationAllowance ?? "No";
            model.AccommodationRequired = request.AccommodationRequired ?? "Not Required";
            model.DependentsCount = request.DependentsCount;
            model.NoticePeriodWeeks = request.NoticePeriodWeeks;
            model.CurrentTaskStatus = request.CurrentTaskStatus ?? string.Empty;
            model.HandoverPlan = request.HandoverPlan ?? string.Empty;
            model.KnowledgeTransferReqd = request.KnowledgeTransferReqd ?? "No";

            model.PrefilledFromRequestId = request.TransferRequestId;
        }
    }
}
