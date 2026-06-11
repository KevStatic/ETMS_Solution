using Dapper;
using ETMS.Application.DTOs.Notifications;
using ETMS.Application.Interfaces;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DapperContext _ctx;
        private readonly IUserSettingsRepository _settings;

        public NotificationRepository(DapperContext ctx, IUserSettingsRepository settings)
        {
            _ctx = ctx;
            _settings = settings;
        }

        public async Task AddAsync(int employeeId, string title, string message, int? transferRequestId = null,
                                   NotificationType type = NotificationType.General)
        {
            // Respect the recipient's notification preferences before creating an alert.
            if (!await ShouldDeliverAsync(employeeId, type))
                return;

            using var conn = _ctx.CreateConnection();
            await conn.ExecuteAsync(@"
                INSERT INTO Notifications (EmployeeId, Title, Message, TransferRequestId)
                VALUES (@EmployeeId, @Title, @Message, @TransferRequestId);",
                new { EmployeeId = employeeId, Title = title, Message = message, TransferRequestId = transferRequestId });
        }

        private async Task<bool> ShouldDeliverAsync(int employeeId, NotificationType type)
        {
            var prefs = await _settings.GetByUserIdAsync(employeeId);

            // Master in-app switch mutes everything.
            if (!prefs.PushInAppAlerts)
                return false;

            return type switch
            {
                NotificationType.TransferStatus => prefs.NotifyTransferStatus,
                NotificationType.ApprovalRequest => prefs.NotifyApprovalRequests,
                NotificationType.LetterReady => prefs.NotifyLetterReady,
                _ => true
            };
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadAsync(int employeeId)
        {
            using var conn = _ctx.CreateConnection();
            return await conn.QueryAsync<NotificationDto>(@"
                SELECT TOP 5 NotificationId, Title, Message, IsRead, CreatedAt, TransferRequestId
                FROM Notifications
                WHERE EmployeeId = @EmployeeId
                ORDER BY CreatedAt DESC;",
                new { EmployeeId = employeeId });
        }

        public async Task<int> GetUnreadCountAsync(int employeeId)
        {
            using var conn = _ctx.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM Notifications
                WHERE EmployeeId = @EmployeeId AND IsRead = 0;",
                new { EmployeeId = employeeId });
        }

        public async Task MarkAllReadAsync(int employeeId)
        {
            using var conn = _ctx.CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE Notifications SET IsRead = 1
                WHERE EmployeeId = @EmployeeId AND IsRead = 0;",
                new { EmployeeId = employeeId });
        }
    }
}