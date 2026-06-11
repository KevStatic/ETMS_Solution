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

        // ✅ New Profile Methods
        public async Task<UserAccount?> GetByUserAccountIdAsync(int userAccountId)
        {
            const string sql = @"
                SELECT UserAccountId, EmployeeId, Username, Password, Role, IsActive
                FROM UserAccounts
                WHERE UserAccountId = @UserAccountId AND IsActive = 1;";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql, new { UserAccountId = userAccountId });
        }

        public async Task<UserAccount?> GetByEmployeeIdAsync(int employeeId)
        {
            const string sql = @"
                SELECT UserAccountId, EmployeeId, Username, Password, Role, IsActive
                FROM UserAccounts
                WHERE EmployeeId = @EmployeeId AND IsActive = 1;";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql, new { EmployeeId = employeeId });
        }

        public async Task<bool> UpdatePasswordByIdAsync(int userAccountId, string newPassword)
        {
            const string sql = @"
                UPDATE UserAccounts
                SET Password = @Password
                WHERE UserAccountId = @UserAccountId AND IsActive = 1;";

            using var connection = _context.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                Password = newPassword,
                UserAccountId = userAccountId
            });
            return rows > 0;
        }

        public async Task<string?> GetLoginEmailByEmployeeIdAsync(int employeeId)
        {
            const string sql = @"
                SELECT COALESCE(NULLIF(Email, ''), Username)
                FROM UserAccounts
                WHERE EmployeeId = @EmployeeId AND IsActive = 1;";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<string?>(sql, new { EmployeeId = employeeId });
        }

        public async Task<bool> UpdatePasswordByEmployeeIdAsync(int employeeId, string newPassword)
        {
            const string sql = @"
                UPDATE UserAccounts
                SET Password = @Password
                WHERE EmployeeId = @EmployeeId AND IsActive = 1;";

            using var connection = _context.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                Password = newPassword,
                EmployeeId = employeeId
            });
            return rows > 0;
        }
    }
}
