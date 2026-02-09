using ETMS.Application.DTOs.Transfer;
using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface ITransferRequestService
    {
        Task<int> ApplyAsync(CreateTransferRequestDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<TransferRequestListItemDto>> GetMineAsync(int employeeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TransferRequestListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(int transferRequestId, string status, int actionByEmployeeId, string? remarks, CancellationToken cancellationToken = default);
        Task<TransferRequest?> GetByIdAsync(int transferRequestId, CancellationToken cancellationToken = default);
    }
}

