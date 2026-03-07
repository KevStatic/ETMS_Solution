namespace ETMS.Application.DTOs.Profile
{
    public class UpdateProfileDto
    {
        public string? Phone { get; set; }
        public string? AlternatePhone { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Address { get; set; }
        public string? BloodGroup { get; set; }
        public string? MaritalStatus { get; set; }

        // ✅ Added missing properties
        public string? Grade { get; set; }
        public string? SBU { get; set; }
        public string? CostCenter { get; set; }
        public string? Company { get; set; }
    }
}