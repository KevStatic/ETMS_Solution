namespace ETMS.Application.DTOs.Auth
{
    public sealed class LoginRequestDto
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public bool RememberMe { get; init; }
    }
}
