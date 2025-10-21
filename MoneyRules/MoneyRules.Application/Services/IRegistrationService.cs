using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Services
{
    public interface IRegistrationService
    {
        Task<User> RegisterAsync(string name, string email, string password);
        Task<User?> LoginAsync(string email, string password);
    }
}
