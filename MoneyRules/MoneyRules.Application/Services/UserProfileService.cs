using Microsoft.EntityFrameworkCore;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Linq;

namespace MoneyRules.Application.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _context;

        public UserProfileService(AppDbContext context)
        {
            _context = context;
        }

        public User GetUserById(int userId)
        {
            return _context.Users
                            .Include(u => u.Settings)
                            .FirstOrDefault(u => u.UserId == userId);
        }

        public void UpdateUser(User user)
        {
            // Якщо Settings ще немає, додаємо його до контексту
            if (user.Settings != null)
            {
                var existingSettings = _context.Settings
                    .FirstOrDefault(s => s.UserId == user.UserId);

                if (existingSettings == null)
                {
                    _context.Settings.Add(user.Settings);
                }
                else
                {
                    _context.Entry(existingSettings).CurrentValues.SetValues(user.Settings);
                }
            }

            // Оновлюємо самого користувача
            _context.Users.Update(user);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new System.Exception("Помилка при збереженні користувача: " + ex.InnerException?.Message ?? ex.Message);
            }
        }

        public void ChangeProfilePhoto(User user, byte[] photoData)
        {
            user.ProfilePhoto = photoData;
            UpdateUser(user);
        }

        // --- ДОДАНО ДЛЯ ТЕМИ ---
        public async Task<Settings?> GetUserSettingsAsync(int userId)
        {
            // Знаходимо користувача разом з його налаштуваннями
            var user = await _context.Users
                                 .Include(u => u.Settings) // "Підтягуємо" пов'язані налаштування
                                 .FirstOrDefaultAsync(u => u.UserId == userId);
            
            return user?.Settings; // Повертаємо налаштування (або null, якщо щось не так)
        }

        public void UpdateUserBudget(int userId, decimal newBudget)
        {
            // 1. Знаходимо ТІЛЬКИ налаштування за ID користувача
            // Це єдиний об'єкт, який DbContext буде відстежувати.
            var settings = _context.Settings.FirstOrDefault(s => s.UserId == userId);

            if (settings == null)
            {
                // 2. Якщо налаштувань не існує - створюємо їх
                settings = new Settings
                {
                    UserId = userId,
                    MonthlyBudget = newBudget,
                    // Встановіть значення за замовчуванням для інших полів,
                    // інакше вони можуть бути null у базі даних
                    Currency = "UAH",
                    NotificationEnabled = false,
                    Theme = "Light"
                };
                _context.Settings.Add(settings);
            }
            else
            {
                // 3. Якщо налаштування існують - просто оновлюємо бюджет
                settings.MonthlyBudget = newBudget;
                _context.Settings.Update(settings);
            }

            // 4. Зберігаємо зміни
            _context.SaveChanges();
        }
    }
}