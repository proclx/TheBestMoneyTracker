using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Services
{
    public interface IAuthService
    {
        Task<User> LoginAsync(string email, string password);
    }
}
