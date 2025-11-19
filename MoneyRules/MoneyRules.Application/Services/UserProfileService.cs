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
            // Використовуйте AsNoTracking() для читання, якщо ви завантажуєте 
            // об'єкт лише для відображення, а потім будете його оновлювати 
            // у новому контексті. Це запобігає можливому ранньому відстеженню.
            return _context.Users
                            .Include(u => u.Settings)
                            .AsNoTracking() // Додано AsNoTracking
                            .FirstOrDefault(u => u.UserId == userId);
        }

        // --------------------------------------------------------------------------------------------------
        // ВИПРАВЛЕНА ВЕРСІЯ: використовує явне відстеження та копіювання значень
        // ЦЕ НАЙБІЛЬШ НАДІЙНИЙ МЕТОД, коли "user" - це від'єднаний об'єкт із UI/API.
        // --------------------------------------------------------------------------------------------------
        public void UpdateUser(User user)
        {
            // 1. Знаходимо вже існуючого користувача та його налаштування у поточному контексті.
            var trackedUser = _context.Users
                                    .Include(u => u.Settings)
                                    .FirstOrDefault(u => u.UserId == user.UserId);

            if (trackedUser == null)
            {
                // Якщо користувача не знайдено, можна або додати, або кинути виняток.
                // Припускаємо, що оновлюємо існуючого:
                throw new System.Exception($"Користувача з ID {user.UserId} не знайдено.");
            }

            // 2. Копіюємо НОВІ ЗНАЧЕННЯ з від'єднаного об'єкта 'user' до відстежуваного 'trackedUser'.
            // Це оновлює лише властивості User, які ви хочете змінити.
            // НЕ використовуємо _context.Users.Update(user)!

            // Оновлення властивостей користувача:
            _context.Entry(trackedUser).CurrentValues.SetValues(user);

            // Якщо об'єкт Settings існує і його потрібно оновити:
            if (user.Settings != null && trackedUser.Settings != null)
            {
                // Оновлення властивостей налаштувань:
                _context.Entry(trackedUser.Settings).CurrentValues.SetValues(user.Settings);
            }
            // Якщо Settings не існувало і його треба додати
            else if (user.Settings != null && trackedUser.Settings == null)
            {
                // Прикріплюємо новий об'єкт Settings до відстежуваного користувача
                trackedUser.Settings = user.Settings;
            }


            try
            {
                // 3. Зберігаємо зміни. EF Core автоматично генерує UPDATE-запити.
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // Зберігаємо вихідну помилку, щоб її було легше діагностувати.
                throw new System.Exception("Помилка при збереженні користувача: " + ex.InnerException?.Message ?? ex.Message);
            }
        }
        // --------------------------------------------------------------------------------------------------
        // Кінець виправленої версії UpdateUser
        // --------------------------------------------------------------------------------------------------


        public void ChangeProfilePhoto(User user, byte[] photoData)
        {
            // Оскільки UpdateUser тепер завантажує та оновлює відстежуваний об'єкт,
            // перед викликом UpdateUser ми повинні знайти відстежувану сутність і оновити її.

            var trackedUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);

            if (trackedUser != null)
            {
                trackedUser.ProfilePhoto = photoData;
                // Не викликаємо UpdateUser(user), оскільки він очікує від'єднаний об'єкт.
                // Просто зберігаємо зміни, оскільки trackedUser вже відстежується.
                _context.SaveChanges();
            }
            else
            {
                throw new System.Exception($"Користувача з ID {user.UserId} не знайдено для оновлення фото.");
            }
        }

        // --- ДОДАНО ДЛЯ ТЕМИ ---
        public async Task<Settings?> GetUserSettingsAsync(int userId)
        {
            // Використовуємо AsNoTracking() для безпечного читання
            var user = await _context.Users
                                    .Include(u => u.Settings)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.UserId == userId);

            return user?.Settings;
        }

        public void UpdateUserBudget(int userId, decimal newBudget)
        {
            // 1. Знаходимо ТІЛЬКИ налаштування за ID користувача
            var settings = _context.Settings.FirstOrDefault(s => s.UserId == userId);

            if (settings == null)
            {
                // 2. Якщо налаштувань не існує - створюємо їх
                settings = new Settings
                {
                    UserId = userId,
                    MonthlyBudget = newBudget,
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
                // _context.Settings.Update(settings) - НЕ ПОТРІБНО, оскільки settings вже відстежується!
            }

            // 4. Зберігаємо зміни
            _context.SaveChanges();
        }
    }
}