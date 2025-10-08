using MoneyRules.Domain.Entities;
using System.Threading.Tasks;

namespace MoneyRules.Application.Services
{
    public interface IAuthService
    {
        Task<User> LoginAsync(string email, string password);
    }
}
