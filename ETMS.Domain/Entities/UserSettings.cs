public class UserSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // --- NOTIFICATION PREFERENCES (transfer workflow) ---
    public bool PushInAppAlerts { get; set; } = true;       // master switch for in-app bell alerts
    public bool NotifyTransferStatus { get; set; } = true;  // my request moved a stage / was rejected
    public bool NotifyApprovalRequests { get; set; } = true;// a request is waiting for my approval
    public bool NotifyLetterReady { get; set; } = true;     // my transfer was fully approved, letter ready

    // --- SECURITY ---
    public bool TwoFactorEnabled { get; set; } = false;

    // --- APPEARANCE ---
    public string Language { get; set; } = "English";
    public string Timezone { get; set; } = "IST";
    public string Theme { get; set; } = "Light";
}
