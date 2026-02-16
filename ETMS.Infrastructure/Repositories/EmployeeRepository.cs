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

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            var query = @"
        SELECT 
            e.*,
            d.*,
            l.*,
            m.*
        FROM Employee e
        LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
        LEFT JOIN Locations l ON e.LocationId = l.LocationId
        LEFT JOIN Employee m ON e.ReportingManagerId = m.EmployeeId
        WHERE e.EmployeeId = @Id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<Employee, Department, Location, Employee, Employee>(
                    query,
                    (emp, dept, loc, manager) =>
                    {
                        emp.Department = dept;
                        emp.Location = loc;
                        emp.ReportingManager = manager;
                        return emp;
                    },
                    new { Id = id },
                    splitOn: "DepartmentId,LocationId,EmployeeId"
                );

                return result.FirstOrDefault();
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
