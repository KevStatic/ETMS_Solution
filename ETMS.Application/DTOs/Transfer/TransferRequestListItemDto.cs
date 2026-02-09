namespace ETMS.Application.DTOs.Transfer
{
    public sealed class TransferRequestListItemDto
    {
        public int TransferRequestId { get; init; }
        public int EmployeeId { get; init; }
        public string EmployeeName { get; init; } = string.Empty;
        public string FromDepartment { get; init; } = string.Empty;
        public string ToDepartment { get; init; } = string.Empty;
        public string FromLocation { get; init; } = string.Empty;
        public string ToLocation { get; init; } = string.Empty;
        public DateTime RequestDate { get; init; }
        public string Status { get; init; } = string.Empty;
        public string TransferType { get; init; } = string.Empty;
    }
}

