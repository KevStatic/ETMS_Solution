<<<<<<< HEAD
﻿namespace ETMS.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
=======
namespace ETMS.Application.DTOs.Auth
{
    public sealed class LoginRequestDto
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public bool RememberMe { get; init; }
    }
}

>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
