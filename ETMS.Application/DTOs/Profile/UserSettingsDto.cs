namespace ETMS.Application.DTOs.Profile
{
    public class UserSettingsDto
    {
        public int UserId { get; set; }
        public bool EmailLeaveNotif { get; set; }
        public bool EmailAttendanceNotif { get; set; }
        public bool EmailPayrollNotif { get; set; }
        public bool PushInAppAlerts { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool ProfileVisible { get; set; }
        public bool ShowOnlineStatus { get; set; }
        public string Language { get; set; } = "English";
        public string Timezone { get; set; } = "IST";
        public string Theme { get; set; } = "Light";
    }
}