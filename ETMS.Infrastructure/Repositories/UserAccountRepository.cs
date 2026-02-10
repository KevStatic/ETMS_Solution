using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;
using System.Threading.Tasks;

namespace ETMS.Infrastructure.Repositories
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly DapperContext _context;

        public UserAccountRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            var sql = "SELECT * FROM Users WHERE Username = @Username";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }
    }
}
