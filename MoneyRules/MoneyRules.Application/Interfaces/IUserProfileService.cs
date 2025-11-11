using MoneyRules.Domain.Entities;
using System.Threading.Tasks; // <-- ДОДАНО, оскільки метод асинхронний

namespace MoneyRules.Application.Interfaces
{
    public interface IUserProfileService
    {
        User GetUserById(int id);
        void UpdateUser(User user);
        void ChangeProfilePhoto(User user, byte[] photoData);

        // --- ДОДАНО ДЛЯ ТЕМИ ---
        Task<Settings?> GetUserSettingsAsync(int userId);
        void UpdateUserBudget(int userId, decimal newBudget);
    }
}