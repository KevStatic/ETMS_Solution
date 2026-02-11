using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Infrastructure.Context;

namespace ETMS.Infrastructure.Repositories
{
    public sealed class LocationRepository : ILocationRepository
    {
        private readonly DapperContext _context;

        public LocationRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Location>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string query = "SELECT LocationId, City, State, Country FROM Locations ORDER BY City, State, Country";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Location>(new CommandDefinition(query, cancellationToken: cancellationToken));
        }

        public async Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string query = "SELECT LocationId, City, State, Country FROM Locations WHERE LocationId = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Location>(new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken));
        }
    }
}

