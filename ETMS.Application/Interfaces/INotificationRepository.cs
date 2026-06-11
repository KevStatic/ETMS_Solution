using ETMS.Application.DTOs.Notifications;

namespace ETMS.Application.Interfaces
{
    public interface INotificationRepository
    {
        /// <summary>
        /// Creates an in-app notification for the employee, unless their notification
        /// preferences mute the master in-app switch or this specific category.
        /// </summary>
        Task AddAsync(int employeeId, string title, string message, int? transferRequestId = null,
                      NotificationType type = NotificationType.General);
        Task<IEnumerable<NotificationDto>> GetUnreadAsync(int employeeId);
        Task<int> GetUnreadCountAsync(int employeeId);
        Task MarkAllReadAsync(int employeeId);
    }
}