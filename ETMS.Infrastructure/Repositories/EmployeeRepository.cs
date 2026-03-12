using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;

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
            const string query = "SELECT * FROM Employee";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Employee>(query);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            // ? Fixed: splitOn uses correct column names
            const string query = @"
                SELECT 
                    e.*,
                    d.DepartmentId, d.DepartmentName,
                    l.LocationId, l.City, l.State, l.Country,
                    m.EmployeeId, m.FirstName, m.LastName
                FROM Employee e
                LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                LEFT JOIN Locations l ON e.LocationId = l.LocationId
                LEFT JOIN Employee m ON e.ReportingManagerId = m.EmployeeId
                WHERE e.EmployeeId = @Id";

            using var connection = _context.CreateConnection();

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
                splitOn: "DepartmentId, LocationId, EmployeeId"  // ? Fixed splitOn
            );

            return result.FirstOrDefault();
        }

        public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
        {
            const string query = "SELECT * FROM Employee WHERE EmployeeCode = @EmployeeCode";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Employee>(
                query, new { EmployeeCode = employeeCode });
        }

        public Task<Employee?> GetByIdAsync(int currentEmployeeId)
        {
            return GetEmployeeByIdAsync(currentEmployeeId);
        }
    }
}