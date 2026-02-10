//using System;
//using System.Collections.Generic;
//using System.Text;

namespace ETMS.Application.Interfaces
{
    public class LoginRequestDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public int? EmployeeId { get; set; }
        public string? Password { get; set; }
        public string? RememberMe { get; set; }
    }
}