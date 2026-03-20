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
    [Authorize(Roles = "Manager,HOD,HR")]
    public class ApprovalController : Controller
    {
        private readonly IApprovalService _approvalSvc;
        private readonly IApprovalDashboardRepository _repo;
        private readonly IWebHostEnvironment _env;

        public ApprovalController(
            IApprovalService approvalSvc,
            IApprovalDashboardRepository repo,
            IWebHostEnvironment env)
        {
            _approvalSvc = approvalSvc;
            _repo = repo;
            _env = env;
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
        public async Task<IActionResult> Approve(ApprovalActionDto model)
        {
            var (approverId, role) = GetCurrentUser();

            if (role == "HR")
            {
                string letterPath = await _approvalSvc.FinaliseAndGenerateLetterAsync(
    model.TransferRequestId, approverId, model.Comments, _env.WebRootPath);

                TempData["SuccessMessage"] = letterPath != null
                    ? "Transfer approved. Letter generated successfully."
                    : "Could not finalise — please check the request status.";
            }
            else
            {
                bool ok = await _approvalSvc.ProcessApprovalAsync(
                    model.TransferRequestId, approverId, role, "Approved", model.Comments);

                TempData["SuccessMessage"] = ok
                    ? "Request approved and forwarded to the next stage."
                    : "Action failed — the request may no longer be at your approval stage.";
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(ApprovalActionDto model)
        {
            var (approverId, role) = GetCurrentUser();

            bool ok = await _approvalSvc.ProcessApprovalAsync(
                model.TransferRequestId, approverId, role, "Rejected", model.Comments);

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

        // ── Download Letter — accessible by all roles ─────────────────
        [Authorize(Roles = "Manager,HOD,HR,Employee")]
        public IActionResult DownloadLetter(int id)
        {
            var letterDir = Path.Combine(_env.WebRootPath, "letters");

            if (!Directory.Exists(letterDir))
                return NotFound("Letters directory not found.");

            var files = Directory.GetFiles(letterDir, $"TL_*_{id}_*.pdf");

            if (!files.Any())
                return NotFound("Letter not yet generated for this request.");

            var filePath = files.First();
            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, "application/pdf", fileName);
        }
    }
}