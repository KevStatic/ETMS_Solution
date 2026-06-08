using Dapper;
using ETMS.Application.DTOs.Profile;
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
                splitOn: "DepartmentId,LocationId,EmployeeId"
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

        // ? Profile Methods
        public async Task<EmployeeProfileDto?> GetProfileByEmployeeIdAsync(int employeeId)
        {
            const string sql = @"
                SELECT
                    e.EmployeeId,
                    e.EmployeeCode,
                    e.FirstName,
                    e.LastName,
                    e.DateOfJoining,
                    e.EmploymentType,
                    e.Grade,
                    e.SBU,
                    e.CostCenter,
                    e.Company,
                    e.HRBP,
                    d.DepartmentName        AS Department,
                    des.Title               AS Designation,
                    b.BranchName            AS Branch,
                    ISNULL(m.FirstName + ' ' + m.LastName, '') AS ReportingManager,
                    ua.Username,
                    ua.Role
                FROM Employee e
                LEFT JOIN Departments  d   ON e.DepartmentId       = d.DepartmentId
                LEFT JOIN Designations des ON e.DesignationId      = des.DesignationId
                LEFT JOIN Branches     b   ON e.BranchId           = b.BranchId
                LEFT JOIN Employee     m   ON e.ReportingManagerId = m.EmployeeId
                INNER JOIN UserAccounts ua ON ua.EmployeeId        = e.EmployeeId
                WHERE e.EmployeeId = @EmployeeId";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<EmployeeProfileDto>(
                sql, new { EmployeeId = employeeId });
        }

        
        public async Task<bool> UpdateProfileAsync(int employeeId, UpdateProfileDto dto)
        {
            const string sql = @"
        UPDATE e SET
            e.FirstName  = @FirstName,
            e.LastName   = @LastName,
            e.Grade      = @Grade,
            e.SBU        = @SBU,
            e.CostCenter = @CostCenter,
            e.Company    = @Company,
            e.HRBP       = @HRBP
        FROM Employee e
        INNER JOIN UserAccounts ua ON ua.EmployeeId = e.EmployeeId
        WHERE e.EmployeeId = @EmployeeId";

            using var connection = _context.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                dto.FirstName,
                dto.LastName,
                dto.Grade,
                dto.SBU,
                dto.CostCenter,
                dto.Company,
                dto.HRBP,
                EmployeeId = employeeId
            });
            return rows > 0;
        }
    }
}
