using ETMS.Application.DTOs.Dashboard;
using ETMS.Application.DTOs.Approval;

namespace ETMS.Application.Interfaces
{
    public interface IApprovalDashboardRepository
    {
        // Metrics
        Task<DashboardMetrics> GetManagerMetricsAsync(int managerId);
        Task<DashboardMetrics> GetHODMetricsAsync(int hodEmployeeId);
        Task<DashboardMetrics> GetHRMetricsAsync();

        // Pending queues
        Task<IEnumerable<PendingApprovalDto>> GetPendingForManagerAsync(int managerId);
        Task<IEnumerable<PendingApprovalDto>> GetPendingForHODAsync(int hodEmployeeId);
        Task<IEnumerable<PendingApprovalDto>> GetPendingForHRAsync();

        // History
        Task<IEnumerable<ActionedRequestDto>> GetActionedByManagerAsync(int managerId, int top = 20);
        Task<IEnumerable<ActionedRequestDto>> GetActionedByHODAsync(int hodId, int top = 20);
        Task<IEnumerable<ActionedRequestDto>> GetAllRequestsAsync(
            string searchTerm, string filterStatus, string sortOrder);

        // Open positions
        Task<IEnumerable<OpenPositionDto>> GetAllOpenPositionsAsync();
        Task AddOpenPositionAsync(string locationName, string departmentName);
        Task RemoveOpenPositionAsync(int positionId);
    }
}