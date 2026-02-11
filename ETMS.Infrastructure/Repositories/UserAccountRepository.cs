<<<<<<< HEAD
﻿using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;
using System.Threading.Tasks;

namespace ETMS.Infrastructure.Repositories
{
    public class UserAccountRepository : IUserAccountRepository
=======
using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public sealed class UserAccountRepository : IUserAccountRepository
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
    {
        private readonly DapperContext _context;

        public UserAccountRepository(DapperContext context)
        {
            _context = context;
        }

<<<<<<< HEAD
        public async Task<User> GetByUsernameAsync(string username)
        {
            var sql = "SELECT * FROM Users WHERE Username = @Username";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }
    }
}
=======
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

>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
