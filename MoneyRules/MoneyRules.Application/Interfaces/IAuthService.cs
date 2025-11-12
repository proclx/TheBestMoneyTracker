using MoneyRules.Domain.Entities;
using MoneyRules.Application.DTOs;
using System.Threading.Tasks;

namespace MoneyRules.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string email, string password, bool rememberMe);
        Task<User?> LoginWithTokenAsync(string token);
        Task<User> RegisterAsync(string name, string email, string password);
        Task ChangePasswordAsync(User user, string newPassword);

        Task<bool> CheckEmailExistsAsync(string email);
        string GenerateConfirmationCode(string email);
        Task<bool> VerifyConfirmationCodeAsync(string email, string code);
        Task<bool> ResetPasswordAsync(string email, string newPassword);

        // --- Новий метод для видалення акаунту ---
        Task<bool> DeleteUserAccountAsync(string email, string password);
    }
}


