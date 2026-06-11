namespace ETMS.Application.DTOs.Notifications
{
    /// <summary>
    /// Categories of in-app notification. Each maps to a user preference toggle on
    /// the Settings page so a recipient can mute a specific kind of alert.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>Always delivered (only gated by the master in-app switch).</summary>
        General = 0,

        /// <summary>The recipient's own request advanced a stage or was rejected.</summary>
        TransferStatus = 1,

        /// <summary>A request is waiting for the recipient to approve (sent to approvers).</summary>
        ApprovalRequest = 2,

        /// <summary>The recipient's transfer was fully approved and the letter is ready.</summary>
        LetterReady = 3
    }
}
