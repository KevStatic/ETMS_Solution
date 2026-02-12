using ETMS.Application.DTOs.Auth;
using ETMS.Application.Interfaces; // Added to find IAuthService
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTransferPortal.Controllers // Or ETMS.Web.Controllers (Check your folder structure)
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        // 1. We inject the Auth Service here so we can check passwords
        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            if (!ModelState.IsValid) return View(model);

            // 2. The Logic from the top section
            var result = await _authService.AuthenticateAsync(model);

            if (result.Success)
            {
                // ============================================================
                // 3. THIS IS THE "LINK" TO YOUR DASHBOARD
                // ============================================================
                // "Index" is the Action, "Dashboard" is your Controller
                return RedirectToAction("Index", "Dashboard");
            }

            // If login fails, stay on page and show error
            ModelState.AddModelError("", result.ErrorMessage);
            return View(model);
        }
    }
}