namespace ETMS.Domain.Entities
{
    public class UserAccount
    {
        public int UserAccountId { get; set; }
        public int EmployeeId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
        public bool IsActive { get; set; } = true;
    }
}

