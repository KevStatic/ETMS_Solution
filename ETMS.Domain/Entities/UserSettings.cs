public class UserSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool EmailLeaveNotif { get; set; } = true;
    public bool EmailAttendanceNotif { get; set; } = true;
    public bool EmailPayrollNotif { get; set; } = true;
    public bool PushInAppAlerts { get; set; } = true;
    public bool TwoFactorEnabled { get; set; } = false;
    public bool ProfileVisible { get; set; } = true;
    public bool ShowOnlineStatus { get; set; } = true;
    public string Language { get; set; } = "English";
    public string Timezone { get; set; } = "IST";
    public string Theme { get; set; } = "Light";
}