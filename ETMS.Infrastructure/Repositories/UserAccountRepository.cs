using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public sealed class UserAccountRepository : IUserAccountRepository
    {
        private readonly DapperContext _context;

        public UserAccountRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT UserAccountId, EmployeeId, Username, Password, Role, IsActive
                FROM UserAccounts
                WHERE Username = @Username AND IsActive = 1;";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<UserAccount>(
                new CommandDefinition(sql, new { Username = username }, cancellationToken: cancellationToken));
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string sql = @"
                SELECT COUNT(1) FROM UserAccounts
                WHERE Username = @Email AND IsActive = 1;";

            using var connection = _context.CreateConnection();
            int count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task UpdatePasswordAsync(string email, string hashedPassword)
        {
            const string sql = @"
                UPDATE UserAccounts
                SET Password = @HashedPassword
                WHERE Username = @Email AND IsActive = 1;";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { HashedPassword = hashedPassword, Email = email });
        }
    }
}