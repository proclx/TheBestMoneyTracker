using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Enums;
using MoneyRules.Application.DTOs; // <-- Додайте це

namespace MoneyRules.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        // --- ПОВНІСТЮ ОНОВЛЕНИЙ МЕТОД LOGINASYNC ---
        public async Task<LoginResult> LoginAsync(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "Email та пароль не можуть бути порожніми." };
            }

            var normalizedEmail = email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "Невірний email або пароль." };
            }

            // Успішний логін, тепер обробляємо "Remember Me"
            string? rememberToken = null;
            if (rememberMe)
            {
                // Генеруємо безпечний токен
                rememberToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                user.RememberMeToken = rememberToken;
                user.RememberMeTokenExpiry = DateTime.UtcNow.AddDays(30); // Токен дійсний 30 днів
            }
            else
            {
                // Якщо "Remember Me" не обрано, скидаємо будь-які старі токени
                user.RememberMeToken = null;
                user.RememberMeTokenExpiry = null;
            }

            await _context.SaveChangesAsync();

            return new LoginResult
            {
                IsSuccess = true,
                User = user,
                RememberMeToken = rememberToken // Повертаємо токен (буде null, якщо rememberMe=false)
            };
        }

        // --- НОВИЙ МЕТОД ---
        public async Task<User?> LoginWithTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RememberMeToken == token &&
                                          u.RememberMeTokenExpiry > DateTime.UtcNow);

            // Якщо токен дійсний, оновимо його, щоб продовжити "сесію"
            if (user != null)
            {
                user.RememberMeTokenExpiry = DateTime.UtcNow.AddDays(30);
                await _context.SaveChangesAsync();
            }

            return user; // Поверне null, якщо токен не знайдено або він прострочений
        }

        // ... ваші існуючі методи RegisterAsync, HashPassword, VerifyPassword, IsValidEmail, ChangePasswordAsync ...
        // ... (скопіюйте їх сюди без змін) ...
        #region Existing Methods
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
                    Currency = "USD",
                    NotificationEnabled = true
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public string HashPassword(string password)
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

        public async Task ChangePasswordAsync(User user, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new ArgumentException("Пароль має містити щонайменше 6 символів.");

            user.PasswordHash = HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        #endregion

        private readonly Dictionary<string, string> _confirmationCodes = new();

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public string GenerateConfirmationCode(string email)
        {
            var code = new Random().Next(100000, 999999).ToString();
            _confirmationCodes[email] = code;
            return code;
        }

        public async Task<bool> VerifyConfirmationCodeAsync(string email, string code)
        {
            return _confirmationCodes.ContainsKey(email) && _confirmationCodes[email] == code;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            user.PasswordHash = HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // видаляємо код підтвердження, щоб не можна було використати повторно
            if (_confirmationCodes.ContainsKey(email))
                _confirmationCodes.Remove(email);

            return true;
        }


    }
}
