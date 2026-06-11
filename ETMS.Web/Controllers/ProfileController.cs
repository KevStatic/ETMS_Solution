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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(UserSettingsDto dto)
        {
            var userId = GetUserId();
            var settings = await _settingsService.GetSettingsAsync(userId);
            settings.UserId = userId;

            ApplyPostedSetting(nameof(UserSettingsDto.PushInAppAlerts), value => settings.PushInAppAlerts = value, dto.PushInAppAlerts);
            ApplyPostedSetting(nameof(UserSettingsDto.NotifyTransferStatus), value => settings.NotifyTransferStatus = value, dto.NotifyTransferStatus);
            ApplyPostedSetting(nameof(UserSettingsDto.NotifyApprovalRequests), value => settings.NotifyApprovalRequests = value, dto.NotifyApprovalRequests);
            ApplyPostedSetting(nameof(UserSettingsDto.NotifyLetterReady), value => settings.NotifyLetterReady = value, dto.NotifyLetterReady);
            ApplyPostedSetting(nameof(UserSettingsDto.TwoFactorEnabled), value => settings.TwoFactorEnabled = value, dto.TwoFactorEnabled);

            if (Request.Form.ContainsKey(nameof(UserSettingsDto.Language)))
                settings.Language = dto.Language;
            if (Request.Form.ContainsKey(nameof(UserSettingsDto.Timezone)))
                settings.Timezone = dto.Timezone;
            if (Request.Form.ContainsKey(nameof(UserSettingsDto.Theme)))
                settings.Theme = dto.Theme;

            await _settingsService.SaveSettingsAsync(userId, settings);
            TempData["Success"] = "Settings saved!";
            return RedirectToAction("Settings");
        }

        private void ApplyPostedSetting(string key, Action<bool> apply, bool value)
        {
            if (Request.Form.ContainsKey(key))
                apply(value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var ok = await _profileService.ChangePasswordAsync(GetUserId(), dto);
            TempData[ok ? "Success" : "Error"] = ok ? "Password changed!" : "Current password is incorrect.";
            return RedirectToAction("Settings");
        }
    }
}
