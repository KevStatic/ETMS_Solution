using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}

