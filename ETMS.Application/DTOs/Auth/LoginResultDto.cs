namespace ETMS.Application.DTOs.Auth
{
    public sealed class LoginResultDto
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public string? DisplayName { get; init; }
        public string? Username { get; init; }
        public int? EmployeeId { get; init; }
        public string? Role { get; init; }
    }
}
