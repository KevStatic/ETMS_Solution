namespace ETMS.Application.DTOs.Transfer
{
    public sealed class CreateTransferRequestDto
    {
        public int EmployeeId { get; init; }
        public int ToDepartmentId { get; init; }
        public int ToLocationId { get; init; }
        public int? NewManagerId { get; init; }
        public string TransferType { get; init; } = "Permanent";
        public string Reason { get; init; } = string.Empty;
        public DateTime? ExpectedRelievingDate { get; init; }
        public DateTime? ExpectedJoiningDate { get; init; }
    }
}

