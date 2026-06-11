namespace ETMS.Application.DTOs.Profile
{
    public class UserSettingsDto
    {
        public int UserId { get; set; }

        // Notification preferences (transfer workflow)
        public bool PushInAppAlerts { get; set; } = true;
        public bool NotifyTransferStatus { get; set; } = true;
        public bool NotifyApprovalRequests { get; set; } = true;
        public bool NotifyLetterReady { get; set; } = true;

        // Security
        public bool TwoFactorEnabled { get; set; }

        // Appearance
        public string Language { get; set; } = "English";
        public string Timezone { get; set; } = "IST";
        public string Theme { get; set; } = "Light";
    }
}
