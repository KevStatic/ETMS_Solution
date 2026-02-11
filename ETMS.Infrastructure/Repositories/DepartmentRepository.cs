using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public sealed class DepartmentRepository : IDepartmentRepository
    {
        private readonly DapperContext _context;

        public DepartmentRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string query = "SELECT DepartmentId, DepartmentName, HeadOfDepartmentId FROM Departments ORDER BY DepartmentName";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Department>(new CommandDefinition(query, cancellationToken: cancellationToken));
        }

        public async Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string query = "SELECT DepartmentId, DepartmentName, HeadOfDepartmentId FROM Departments WHERE DepartmentId = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Department>(new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken));
        }
    }
}

