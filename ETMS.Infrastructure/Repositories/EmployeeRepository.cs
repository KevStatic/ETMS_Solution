using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;
using System.Threading.Tasks;

namespace ETMS.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DapperContext _context;

        public EmployeeRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            var sql = "SELECT * FROM Employees WHERE EmployeeId = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
        }
    }
}
