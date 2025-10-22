using Microsoft.EntityFrameworkCore;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
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
    }
}
