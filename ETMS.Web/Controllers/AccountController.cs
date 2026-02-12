//using ETMS.Application.Interfaces;
//using Microsoft.AspNetCore.Mvc;

//namespace ETMS.Web.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly IAuthService _authService;

//        public AccountController(IAuthService authService) => _authService = authService;

//        [HttpGet]
//        public IActionResult Login() => View();

//        [HttpPost]
//        public async Task<IActionResult> Login(LoginRequestDto model)
//        {
//            if (!ModelState.IsValid) return View(model);

//            var result = await _authService.AuthenticateAsync(model);
//            if (result.Success)
//            {
//                // Set up Authentication Cookies or Session here
//                return RedirectToAction("Index", "Home");
//            }

//            ModelState.AddModelError("", result.ErrorMessage);
//            return View(model);
//        }
//    }
//}

using ETMS.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTransferPortal.Controllers
{
    [Route("login")] // This makes the URL https://localhost:7179/login
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            // Your existing login logic
            return View(model);
        }
    }
}
