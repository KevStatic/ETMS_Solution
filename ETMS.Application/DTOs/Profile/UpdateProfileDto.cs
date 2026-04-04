namespace ETMS.Application.DTOs.Profile
{
    public class UpdateProfileDto
    {
        // ── Editable fields (saved to DB)
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Grade { get; set; }
        public string? SBU { get; set; }
        public string? CostCenter { get; set; }
        public string? Company { get; set; }
        public string? HRBP { get; set; }

        // ── Extra fields (kept for future use)
        public string? Phone { get; set; }
        public string? AlternatePhone { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Address { get; set; }
        public string? BloodGroup { get; set; }
        public string? MaritalStatus { get; set; }
    }
}