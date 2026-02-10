using System;

namespace ETMS.Application.DTOs.Transfer
{
    public class TransferRequestDto
    {
        public int TransferRequestId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;

        public string? EmployeeName { get; set; }
        public string? FromDepartment { get; set; }
        public string? ToDepartment { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
    }
}
