using ETMS.Application.DTOs.Transfer;

namespace ETMS.Web.Models
{
    public class DashboardViewModel
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public IEnumerable<TransferRequestListItemDto> Requests { get; set; }
            = new List<TransferRequestListItemDto>();
    }
}