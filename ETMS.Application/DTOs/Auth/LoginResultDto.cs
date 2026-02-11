<<<<<<< HEAD
﻿namespace ETMS.Application.DTOs.Auth
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
=======
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

>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
