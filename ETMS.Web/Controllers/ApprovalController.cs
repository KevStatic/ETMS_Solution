using ETMS.Application.DTOs.Approval;
using ETMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace ETMS.Web.Controllers
{
    // Any authenticated user may reach this controller; individual actions below
    // tighten access. (A controller-level role filter combines with action-level
    // ones using AND, so it would block employees from downloading their own letter.)
    [Authorize]
    public class ApprovalController : Controller
    {
        private readonly IApprovalService _approvalSvc;
        private readonly IApprovalDashboardRepository _repo;
        private readonly IWebHostEnvironment _env;
        private readonly IUrlEncryptionService _enc;
        private readonly ITransferRequestRepository _transferRepo;

        public ApprovalController(
            IApprovalService approvalSvc,
            IApprovalDashboardRepository repo,
            IWebHostEnvironment env,
            IUrlEncryptionService enc,
            ITransferRequestRepository transferRepo)
        {
            _approvalSvc = approvalSvc;
            _repo = repo;
            _env = env;
            _enc = enc;
            _transferRepo = transferRepo;
        }

        private (int employeeId, string role) GetCurrentUser()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            int.TryParse(idClaim?.Value, out int id);
            return (id, roleClaim?.Value ?? "Employee");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,HOD,HR")]
        public async Task<IActionResult> Approve(ApprovalActionDto model)
        {
            var (approverId, role) = GetCurrentUser();

            var realId = _enc.Decrypt(model.TransferRequestId);
            if (realId == -1)
            {
                TempData["SuccessMessage"] = "Invalid or tampered request.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (role == "HR")
            {
                string letterPath = await _approvalSvc.FinaliseAndGenerateLetterAsync(
                    realId, approverId, model.Comments, _env.WebRootPath);

                TempData["SuccessMessage"] = letterPath != null
                    ? "Transfer approved. Letter generated successfully."
                    : "Could not finalise — please check the request status.";
            }
            else
            {
                bool ok = await _approvalSvc.ProcessApprovalAsync(
                    realId, approverId, role, "Approved", model.Comments);

                TempData["SuccessMessage"] = ok
                    ? "Request approved and forwarded to the next stage."
                    : "Action failed — the request may no longer be at your approval stage.";
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,HOD,HR")]
        public async Task<IActionResult> Reject(ApprovalActionDto model)
        {
            var (approverId, role) = GetCurrentUser();

            var realId = _enc.Decrypt(model.TransferRequestId);
            if (realId == -1)
            {
                TempData["SuccessMessage"] = "Invalid or tampered request.";
                return RedirectToAction("Index", "Dashboard");
            }

            bool ok = await _approvalSvc.ProcessApprovalAsync(
                realId, approverId, role, "Rejected", model.Comments);

            TempData["SuccessMessage"] = ok
                ? "Request rejected."
                : "Action failed — the request may no longer be at your approval stage.";

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,HOD")]
        public async Task<IActionResult> AddPosition(UpdatePositionDto model)
        {
            await _repo.AddOpenPositionAsync(model.LocationName, model.DepartmentName);
            TempData["SuccessMessage"] = $"Position added: {model.DepartmentName} @ {model.LocationName}";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,HOD")]
        public async Task<IActionResult> RemovePosition(int positionId)
        {
            await _repo.RemoveOpenPositionAsync(positionId);
            TempData["SuccessMessage"] = "Position removed.";
            return RedirectToAction("Index", "Dashboard");
        }

        // ── Download Letter — the request owner or any approver role ──────────
        [Authorize]
        public async Task<IActionResult> DownloadLetter(string id)
        {
            var realId = _enc.Decrypt(id);
            if (realId == -1) return BadRequest("Invalid or tampered request.");

            var (currentUserId, role) = GetCurrentUser();
            bool isApprover = role is "Manager" or "HOD" or "HR";

            // Employees may only download the letter for their own request.
            if (!isApprover)
            {
                var request = await _transferRepo.GetByIdAsync(realId);
                if (request == null || request.EmployeeId != currentUserId)
                    return NotFound("Letter not found for this request.");
            }

            var letterDir = Path.Combine(_env.WebRootPath, "letters");

            if (!Directory.Exists(letterDir))
                return NotFound("Letters directory not found.");

            var files = Directory.GetFiles(letterDir, $"TL_*_{realId}_*.pdf");

            if (!files.Any())
                return NotFound("Letter not yet generated for this request.");

            var filePath = files.First();
            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, "application/pdf", fileName);
        }
    }
}