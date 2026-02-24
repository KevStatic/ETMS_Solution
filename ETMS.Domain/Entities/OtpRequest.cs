namespace ETMS.Domain.Entities
{
    // Place this file in: ETMS.Domain > Entities > OtpRequest.cs
    public class OtpRequest
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string OtpCode { get; set; } = string.Empty;  // BCrypt hash
        public string Channel { get; set; } = string.Empty;  // "email" or "phone"
        public bool IsVerified { get; set; } = false;
        public bool IsUsed { get; set; } = false;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}