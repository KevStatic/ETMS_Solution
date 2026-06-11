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
        private readonly IUserSettingsService _settingsService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IUserAccountRepository _userAccountRepo;

        public AccountController(
            IAuthService authService,
            IForgotPasswordService forgotPasswordService,
            IUserSettingsService settingsService,
            ITwoFactorService twoFactorService,
            IUserAccountRepository userAccountRepo)
        {
            _authService = authService;
            _forgotPasswordService = forgotPasswordService;
            _settingsService = settingsService;
            _twoFactorService = twoFactorService;
            _userAccountRepo = userAccountRepo;
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

            var employeeId = result.EmployeeId!.Value;

            // ── Two-factor gate ──────────────────────────────────────────────
            // If the account has 2FA enabled, the password is only the first factor.
            // Don't issue the auth cookie yet — send an email OTP and require it.
            var settings = await _settingsService.GetSettingsAsync(employeeId);
            if (settings.TwoFactorEnabled)
            {
                string email = await ResolveLoginEmailAsync(employeeId, result.Username!);
                var (sent, message) = await _twoFactorService.SendLoginOtpAsync(email);

                if (!sent)
                {
                    ModelState.AddModelError(string.Empty, message);
                    ViewData["ReturnUrl"] = returnUrl;
                    return View(model);
                }

                TempData["2fa_EmployeeId"] = employeeId;
                TempData["2fa_Username"] = result.Username;
                TempData["2fa_Role"] = result.Role;
                TempData["2fa_Email"] = email;
                TempData["2fa_Remember"] = model.RememberMe;
                TempData["2fa_ReturnUrl"] = returnUrl;
                TempData["2fa_Info"] = message;
                return RedirectToAction(nameof(TwoFactor));
            }

            await SignInAsync(employeeId, result.Username!, result.Role!, model.RememberMe);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Account/TwoFactor
        [HttpGet]
        public IActionResult TwoFactor()
        {
            if (TempData["2fa_EmployeeId"] == null)
                return RedirectToAction(nameof(Login));

            ViewBag.Info = TempData["2fa_Info"];
            ViewBag.Email = TempData["2fa_Email"];
            TempData.Keep();
            return View();
        }

        // POST: /Account/TwoFactor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactor(string code)
        {
            if (TempData["2fa_EmployeeId"] == null)
                return RedirectToAction(nameof(Login));

            // Pull the pending login context before TempData is consumed.
            var employeeId = (int)TempData["2fa_EmployeeId"]!;
            var username = TempData["2fa_Username"]?.ToString() ?? string.Empty;
            var role = TempData["2fa_Role"]?.ToString() ?? "Employee";
            var email = TempData["2fa_Email"]?.ToString() ?? string.Empty;
            var remember = TempData["2fa_Remember"] is true;
            var returnUrl = TempData["2fa_ReturnUrl"]?.ToString();

            var (ok, message) = await _twoFactorService.VerifyLoginOtpAsync(email, code);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Email = email;
                // Keep the context so the user can retry on the same page.
                TempData["2fa_EmployeeId"] = employeeId;
                TempData["2fa_Username"] = username;
                TempData["2fa_Role"] = role;
                TempData["2fa_Email"] = email;
                TempData["2fa_Remember"] = remember;
                TempData["2fa_ReturnUrl"] = returnUrl;
                return View();
            }

            await SignInAsync(employeeId, username, role, remember);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        private async Task SignInAsync(int employeeId, string username, string role, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employeeId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private async Task<string> ResolveLoginEmailAsync(int employeeId, string fallbackUsername)
        {
            try
            {
                var email = await _userAccountRepo.GetLoginEmailByEmployeeIdAsync(employeeId);
                return string.IsNullOrWhiteSpace(email) ? fallbackUsername : email;
            }
            catch
            {
                // Email column may not exist on an un-migrated database.
                return fallbackUsername;
            }
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