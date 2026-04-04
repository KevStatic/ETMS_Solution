namespace ETMS.Application.DTOs.Profile
{
    public class EmployeeProfileDto
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string? EmploymentType { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Branch { get; set; }
        public string? ReportingManager { get; set; }
        public string? Grade { get; set; }
        public string? SBU { get; set; }
        public string? CostCenter { get; set; }
        public string? Company { get; set; }
        public string? HRBP { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }
}