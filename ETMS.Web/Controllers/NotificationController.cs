using ETMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETMS.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _repo;
        public NotificationController(INotificationRepository repo) => _repo = repo;

        public async Task<IActionResult> MarkRead()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int empId))
                await _repo.MarkAllReadAsync(empId);

            return RedirectToAction("Index", "Dashboard");
        }
    }
}