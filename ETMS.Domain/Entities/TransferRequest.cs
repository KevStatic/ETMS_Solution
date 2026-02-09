namespace ETMS.Domain.Entities
{
    public class TransferRequest
    {
        public int TransferRequestId { get; set; }
        public int EmployeeId { get; set; }
        public int FromDepartmentId { get; set; }
        public int ToDepartmentId { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public int? OldManagerId { get; set; }
        public int? NewManagerId { get; set; }
        public string TransferType { get; set; } = "Permanent";
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? ExpectedRelievingDate { get; set; }
        public DateTime? ExpectedJoiningDate { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsActive { get; set; } = true;
    }
}
