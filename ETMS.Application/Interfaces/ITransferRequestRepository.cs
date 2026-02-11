<<<<<<< HEAD
﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ETMS.Application.DTOs.Transfer;
=======
using ETMS.Application.DTOs.Transfer;
using ETMS.Domain.Entities;
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e

namespace ETMS.Application.Interfaces
{
    public interface ITransferRequestRepository
    {
<<<<<<< HEAD
        Task<IEnumerable<TransferRequestDto>> GetByEmployeeIdAsync(int employeeId);
    }
}
=======
        Task<int> CreateAsync(TransferRequest request, CancellationToken cancellationToken = default);
        Task<TransferRequest?> GetByIdAsync(int transferRequestId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TransferRequestListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TransferRequestListItemDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(int transferRequestId, string status, int actionByEmployeeId, string? remarks, CancellationToken cancellationToken = default);
        Task<IEnumerable<TransferRequest>> GetAllPendingAsync();
    }
}

>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
