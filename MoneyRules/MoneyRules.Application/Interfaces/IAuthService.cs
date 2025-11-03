using MoneyRules.Domain.Entities;
using MoneyRules.Application.DTOs;

namespace MoneyRules.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string email, string password, bool rememberMe);
        Task<User?> LoginWithTokenAsync(string token);
        Task<User> RegisterAsync(string name, string email, string password);
        Task ChangePasswordAsync(User user, string newPassword);
    }
}

