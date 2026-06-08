using Dapper;
using ETMS.Application.DTOs.Notifications;
using ETMS.Application.Interfaces;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DapperContext _ctx;
        public NotificationRepository(DapperContext ctx) => _ctx = ctx;

        public async Task AddAsync(int employeeId, string title, string message, int? transferRequestId = null)
        {
            using var conn = _ctx.CreateConnection();
            await conn.ExecuteAsync(@"
                INSERT INTO Notifications (EmployeeId, Title, Message, TransferRequestId)
                VALUES (@EmployeeId, @Title, @Message, @TransferRequestId);",
                new { EmployeeId = employeeId, Title = title, Message = message, TransferRequestId = transferRequestId });
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