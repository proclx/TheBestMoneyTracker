using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.DTOs
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public User? User { get; set; }
        public string? RememberMeToken { get; set; }
        public string? ErrorMessage { get; set; }
    }
}