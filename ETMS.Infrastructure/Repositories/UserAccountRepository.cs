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
    }
}

