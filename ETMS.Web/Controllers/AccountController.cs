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

using Microsoft.AspNetCore.Mvc;

namespace EmployeeTransferPortal.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();

        }
    }
}