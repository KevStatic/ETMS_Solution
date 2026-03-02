using System;

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
        public string TransferType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? ExpectedRelievingDate { get; set; }
        public DateTime? ExpectedJoiningDate { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsActive { get; set; }

        // --- NEW L&T FORM FIELDS ---
        public string? LetterType { get; set; }
        public string? WithinCity { get; set; }
        public string? RelocationStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ProjectName { get; set; }
        public string? NewVertical { get; set; }
        public string? NewBU { get; set; }
        public string? NewISPsno { get; set; }
        public string? NewISName { get; set; }
        public string? NewISEmail { get; set; }
        public string? ICHead { get; set; }
        public string? Remarks { get; set; }
    }
}