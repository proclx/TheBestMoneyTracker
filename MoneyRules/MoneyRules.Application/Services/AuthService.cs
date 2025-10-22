using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;

namespace MoneyRules.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Вхід користувача — перевірка email і пароля.
        /// </summary>
        public async Task<User?> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Email та пароль не можуть бути порожніми.");

            var normalizedEmail = email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
                return null;

            return VerifyPassword(password, user.PasswordHash) ? user : null;
        }

        /// <summary>
        /// Реєстрація нового користувача.
        /// </summary>
        public async Task<User> RegisterAsync(string name, string email, string password)
        {
            if (!IsValidEmail(email))
                throw new ArgumentException("Невірний формат email.");

            if (password.Length < 6)
                throw new ArgumentException("Пароль має містити щонайменше 6 символів.");

            var normalizedEmail = email.Trim().ToLower();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (existingUser != null)
                throw new InvalidOperationException("Користувач з таким email вже існує.");

            var passwordHash = HashPassword(password);

            var user = new User
            {
                Name = name.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = UserRole.User,
                ProfilePhoto = Array.Empty<byte>(),
                Settings = new Settings
                {
                    Currency = "UAH",
                    NotificationEnabled = true
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // --- ДОПОМІЖНІ МЕТОДИ ---

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] stored = Convert.FromBase64String(parts[1]);

            byte[] computed = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);

            return CryptographicOperations.FixedTimeEquals(stored, computed);
        }

        private bool IsValidEmail(string email)
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
    }
}
