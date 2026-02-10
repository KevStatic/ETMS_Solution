namespace ETMS.Application.DTOs.Auth
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
    }
}
