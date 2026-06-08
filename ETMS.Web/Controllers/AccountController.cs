using ETMS.Application.DTOs;
using ETMS.Application.DTOs.Auth;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// ✅ FIXED: namespace matches ETMS.Web project
namespace ETMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IForgotPasswordService _forgotPasswordService;

        public AccountController(
            IAuthService authService,
            IForgotPasswordService forgotPasswordService)
        {
            _authService = authService;
            _forgotPasswordService = forgotPasswordService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var result = await _authService.AuthenticateAsync(model);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError(string.Empty, result?.ErrorMessage ?? "Invalid login attempt.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, result.EmployeeId.ToString()),
        new Claim(ClaimTypes.Name, result.Username),
        new Claim(ClaimTypes.Role, result.Role)
    };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new SendOtpDto());
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(SendOtpDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, message) = await _forgotPasswordService.SendOtpAsync(dto);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(dto);
            }

            TempData["Email"] = dto.Email;
            TempData["SuccessMessage"] = message;

            return RedirectToAction("VerifyOtp");
        }

        // GET: /Account/VerifyOtp
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (TempData["Email"] == null)
                return RedirectToAction("ForgotPassword");

            var dto = new VerifyOtpDto
            {
                Email = TempData["Email"]!.ToString()!
            };

            TempData.Keep();
            ViewBag.InfoMessage = TempData["SuccessMessage"];

            return View(dto);
        }

        // POST: /Account/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, message, resetToken) = await _forgotPasswordService.VerifyOtpAsync(dto);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(dto);
            }

            TempData["ResetToken"] = resetToken;
            return RedirectToAction("ResetPassword");
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["ResetToken"] == null)
                return RedirectToAction("ForgotPassword");

            var dto = new ResetPasswordDto
            {
                ResetToken = TempData["ResetToken"]!.ToString()!
            };

            TempData.Keep();
            return View(dto);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, message) = await _forgotPasswordService.ResetPasswordAsync(dto);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(dto);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // 1. Clear the secure authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Clear any temporary messages or session data
            TempData.Clear();

            // 3. Redirect them straight to the Login screen
            return RedirectToAction("Login", "Account");
        }
    }
}