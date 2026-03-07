using ETMS.Application.DTOs.Profile;
using ETMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETMS.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IUserSettingsService _settingsService;

        public ProfileController(IProfileService profileService, IUserSettingsService settingsService)
        {
            _profileService = profileService;
            _settingsService = settingsService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        public async Task<IActionResult> MyProfile()
        {
            var profile = await _profileService.GetProfileAsync(GetUserId());
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            await _profileService.UpdateProfileAsync(GetUserId(), dto);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("MyProfile");
        }

        public async Task<IActionResult> Settings()
        {
            var settings = await _settingsService.GetSettingsAsync(GetUserId());
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSettings(UserSettingsDto dto)
        {
            await _settingsService.SaveSettingsAsync(GetUserId(), dto);
            TempData["Success"] = "Settings saved!";
            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var ok = await _profileService.ChangePasswordAsync(GetUserId(), dto);
            TempData[ok ? "Success" : "Error"] = ok ? "Password changed!" : "Current password is incorrect.";
            return RedirectToAction("Settings");
        }
    }
}