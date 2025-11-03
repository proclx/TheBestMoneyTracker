using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums; // <-- Можливо, знадобиться

namespace MoneyRules.Tests.Services
{
    public class AuthServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_NewUser_SavesAndReturnsUser()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var user = await service.RegisterAsync("John Doe", "john@example.com", "123456");

            Assert.NotNull(user);
            Assert.Equal("john@example.com", user.Email);
            Assert.NotNull(user.Settings);
            Assert.Equal("USD", user.Settings.Currency);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessfulResult()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var registeredUser = await service.RegisterAsync("Mary", "mary@example.com", "123456");

            // --- ЗМІНЕНО ---
            // Викликаємо з rememberMe: false
            var result = await service.LoginAsync("mary@example.com", "123456", false);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess); // <-- Перевіряємо результат
            Assert.NotNull(result.User);
            Assert.Equal("mary@example.com", result.User.Email);
            Assert.Null(result.RememberMeToken); // <-- Токена не повинно бути
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsFailedResult()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            await service.RegisterAsync("Tom", "tom@example.com", "correctpass");

            // --- ЗМІНЕНО ---
            var result = await service.LoginAsync("tom@example.com", "wrongpass", false);

            Assert.NotNull(result);
            Assert.False(result.IsSuccess); // <-- Перевіряємо результат
            Assert.Null(result.User);
            Assert.Equal("Невірний email або пароль.", result.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_UserNotExists_ReturnsFailedResult()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            // --- ЗМІНЕНО ---
            var result = await service.LoginAsync("unknown@example.com", "123456", false);

            Assert.NotNull(result);
            Assert.False(result.IsSuccess); // <-- Перевіряємо результат
            Assert.Null(result.User);
        }

        [Fact]
        public async Task RegisterAsync_InvalidEmail_ThrowsException()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.RegisterAsync("Invalid Email", "invalid-email", "123456"));
        }

        [Fact]
        public async Task RegisterAsync_ShortPassword_ThrowsException()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.RegisterAsync("User", "user@example.com", "123"));
        }

        // --- ОНОВЛЕНІ ТЕСТИ ЗМІНИ ПАРОЛЮ ---

        [Fact]
        public async Task ChangePasswordAsync_LoginAfterPasswordChange_WorksWithNewPassword()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var user = await service.RegisterAsync("Bob", "bob@example.com", "Initial123");

            // Перевіряємо, що старий пароль працює
            var loginOld = await service.LoginAsync("bob@example.com", "Initial123", false);
            Assert.True(loginOld.IsSuccess);
            Assert.NotNull(loginOld.User);

            // Змінюємо пароль
            await service.ChangePasswordAsync(user, "Changed456");

            // Act
            var loginWithOldPassword = await service.LoginAsync("bob@example.com", "Initial123", false);
            var loginWithNewPassword = await service.LoginAsync("bob@example.com", "Changed456", false);

            // Assert
            Assert.False(loginWithOldPassword.IsSuccess); // <-- Старий пароль не працює
            Assert.True(loginWithNewPassword.IsSuccess);  // <-- Новий пароль працює
            Assert.NotNull(loginWithNewPassword.User);
        }

        // --- НОВІ ТЕСТИ ДЛЯ "REMEMBER ME" ---

        [Fact]
        public async Task LoginAsync_WithRememberMeTrue_GeneratesAndReturnsToken()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            await service.RegisterAsync("Alice", "alice@example.com", "123456");

            var result = await service.LoginAsync("alice@example.com", "123456", true);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.RememberMeToken);

            // Перевіряємо, що токен збережено в БД
            var userInDb = await context.Users.FirstAsync(u => u.Email == "alice@example.com");
            Assert.Equal(result.RememberMeToken, userInDb.RememberMeToken);
            Assert.NotNull(userInDb.RememberMeTokenExpiry);
            Assert.True(userInDb.RememberMeTokenExpiry > DateTime.UtcNow.AddDays(29)); // Більше 29 днів
        }

        [Fact]
        public async Task LoginAsync_WithRememberMeFalse_ClearsExistingToken()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            var user = await service.RegisterAsync("Alice", "alice@example.com", "123456");

            // Встановлюємо "старий" токен
            user.RememberMeToken = "old-token";
            user.RememberMeTokenExpiry = DateTime.UtcNow.AddDays(10);
            await context.SaveChangesAsync();

            // Входимо з rememberMe: false
            var result = await service.LoginAsync("alice@example.com", "123456", false);

            Assert.True(result.IsSuccess);
            Assert.Null(result.RememberMeToken); // Токен не повернуто

            // Перевіряємо, що токен очищено в БД
            var userInDb = await context.Users.FirstAsync(u => u.Email == "alice@example.com");
            Assert.Null(userInDb.RememberMeToken);
            Assert.Null(userInDb.RememberMeTokenExpiry);
        }

        [Fact]
        public async Task LoginWithTokenAsync_ValidToken_ReturnsUser()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            await service.RegisterAsync("Alice", "alice@example.com", "123456");

            // Входимо, щоб отримати токен
            var loginResult = await service.LoginAsync("alice@example.com", "123456", true);
            var token = loginResult.RememberMeToken;

            // Входимо за токеном
            var user = await service.LoginWithTokenAsync(token);

            Assert.NotNull(user);
            Assert.Equal("alice@example.com", user.Email);
        }

        [Fact]
        public async Task LoginWithTokenAsync_InvalidToken_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var user = await service.LoginWithTokenAsync("invalid-fake-token");

            Assert.Null(user);
        }

        [Fact]
        public async Task LoginWithTokenAsync_ExpiredToken_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            var user = await service.RegisterAsync("Alice", "alice@example.com", "123456");

            // Встановлюємо прострочений токен
            user.RememberMeToken = "expired-token";
            user.RememberMeTokenExpiry = DateTime.UtcNow.AddMinutes(-10); // 10 хвилин тому
            await context.SaveChangesAsync();

            // Намагаємось увійти за токеном
            var resultUser = await service.LoginWithTokenAsync("expired-token");

            Assert.Null(resultUser);
        }
    }
}
