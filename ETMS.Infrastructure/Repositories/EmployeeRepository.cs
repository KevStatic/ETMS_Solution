using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;
using System.Collections.Generic;
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

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            var query = "SELECT * FROM Employee";

            using (var connection = _context.CreateConnection())
            {
                var employees = await connection.QueryAsync<Employee>(query);
                return employees;
            }
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            var query = "SELECT * FROM Employee WHERE EmployeeId = @Id";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Employee>(query, new { Id = id });
            }
        }

        public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
        {
            var query = "SELECT * FROM Employee WHERE EmployeeCode = @EmployeeCode";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Employee>(query, new { EmployeeCode = employeeCode });
            }
        }

        public async Task GetByIdAsync(int currentEmployeeId)
        {
            // Implementation placeholder: method required by IEmployeeRepository.
            // No return value specified, so just complete as a no-op or throw if not needed.
            await Task.CompletedTask;
        }
    }
}