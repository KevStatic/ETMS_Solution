using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface ILocationRepository
    {
        Task<IEnumerable<Location>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}

