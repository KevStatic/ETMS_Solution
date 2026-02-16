using ETMS.Application.DTOs.Auth;
using ETMS.Application.Interfaces; // Added to find IAuthService
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                // 2. CREATE THE USER SESSION (COOKIES)
                // This is how the server remembers "Keval is logged in"
                var claims = new List<Claim>
                {
                    // Store the Username
                    new Claim(ClaimTypes.Name, result.Username),
            
                    // Store the Role (Employee)
                    new Claim(ClaimTypes.Role, result.Role),

                    // IMPORTANT: Store the EmployeeId (1) so Dashboard can read it
                    new Claim(ClaimTypes.NameIdentifier, result.EmployeeId.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                // 3. ACTUAL LOGIN STEP
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // 4. Redirect to Dashboard
                return RedirectToAction("Index", "Dashboard");
            }

            // If login fails, stay on page and show error
            ModelState.AddModelError("", result.ErrorMessage);
            return View(model);
        }
    }
}