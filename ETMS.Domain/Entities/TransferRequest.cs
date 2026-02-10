using System;

namespace ETMS.Domain.Entities
{
    public class TransferRequest
    {
        public int TransferRequestId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
        public int FromDepartmentId { get; set; }
        public int ToDepartmentId { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public bool IsActive { get; set; }
    }
}
