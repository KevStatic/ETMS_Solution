using ETMS.Application.DTOs.Notifications;

namespace ETMS.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(int employeeId, string title, string message, int? transferRequestId = null);
        Task<IEnumerable<NotificationDto>> GetUnreadAsync(int employeeId);
        Task<int> GetUnreadCountAsync(int employeeId);
        Task MarkAllReadAsync(int employeeId);
    }
}