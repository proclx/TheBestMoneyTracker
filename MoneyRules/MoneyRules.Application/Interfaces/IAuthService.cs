using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User> LoginAsync(string email, string password);
    }
}
