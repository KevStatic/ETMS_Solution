using ETMS.Domain.Entities;
using ETMS.Application.DTOs.Dashboard;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ETMS.Application.Interfaces
{
    public interface ITransferRequestRepository
    {
        Task<int> CreateAsync(TransferRequest request, CancellationToken cancellationToken = default);
        Task<TransferRequest?> GetByIdAsync(int transferRequestId, CancellationToken cancellationToken = default);

        // FIX: Explicitly specifying the full path to remove the ambiguity error
        Task<IEnumerable<ETMS.Application.DTOs.Transfer.TransferRequestListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ETMS.Application.DTOs.Transfer.TransferRequestListItemDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ETMS.Application.DTOs.Transfer.TransferRequestListItemDto>> GetPendingApprovalsForManagerAsync(int managerEmployeeId, CancellationToken cancellationToken = default);

        Task UpdateStatusAsync(int transferRequestId, string status, int actionByEmployeeId, string? remarks, CancellationToken cancellationToken = default);
        Task<IEnumerable<TransferRequest>> GetAllPendingAsync();
        Task<int> AddAsync(TransferRequest request);
        Task<DashboardMetrics> GetDashboardMetricsAsync(int employeeId);
        Task CancelAsync(int id, int employeeId, CancellationToken cancellationToken = default);
    }
}
