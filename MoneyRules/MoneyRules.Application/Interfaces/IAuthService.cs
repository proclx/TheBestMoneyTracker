using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User> LoginAsync(string email, string password);
        Task<User> RegisterAsync(string name, string email, string password);
    }
}
