using System.Collections.Generic;
using System.Threading.Tasks;
using ETMS.Application.DTOs.Transfer;

namespace ETMS.Application.Interfaces
{
    public interface ITransferRequestRepository
    {
        Task<IEnumerable<TransferRequestDto>> GetByEmployeeIdAsync(int employeeId);
    }
}
