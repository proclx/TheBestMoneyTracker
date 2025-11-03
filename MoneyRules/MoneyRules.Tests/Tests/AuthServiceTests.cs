using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Application.Interfaces; // <-- Потрібно для LoginResult

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
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessResult()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            await service.RegisterAsync("Mary", "mary@example.com", "123456");

            // Act
            var result = await service.LoginAsync("mary@example.com", "123456", false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.User);
            Assert.Equal("mary@example.com", result.User.Email);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsFailResult()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            await service.RegisterAsync("Tom", "tom@example.com", "correctpass");

            // Act
            var result = await service.LoginAsync("tom@example.com", "wrongpass", false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Null(result.User);
            Assert.Equal("Невірний email або пароль.", result.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_UserNotExists_ReturnsFailResult()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            // Act
            var result = await service.LoginAsync("unknown@example.com", "123456", false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
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

        // ------------------- Тести зміни паролю (Виправлено) -------------------

        [Fact]
        public async Task ChangePasswordAsync_UpdatesPasswordHashSuccessfully()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var user = await service.RegisterAsync("Alice", "alice@example.com", "OldPass123");
            var oldHash = user.PasswordHash;
            var newPassword = "NewPass456";

            // Act
            // Викликаємо публічний метод, а не робимо логіку тесту
            await service.ChangePasswordAsync(user, newPassword);

            var updatedUser = await context.Users.FindAsync(user.UserId);

            // Assert
            Assert.NotNull(updatedUser);
            Assert.NotEqual(oldHash, updatedUser.PasswordHash);

            // Перевіряємо логін з новим паролем
            var loginResult = await service.LoginAsync("alice@example.com", newPassword, false);
            Assert.True(loginResult.IsSuccess);
        }

        [Fact]
        public async Task LoginAfterPasswordChange_WorksWithNewPassword()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var user = await service.RegisterAsync("Bob", "bob@example.com", "Initial123");

            // Change password
            await service.ChangePasswordAsync(user, "Changed456");

            // Act
            var loginWithOldPassword = await service.LoginAsync("bob@example.com", "Initial123", false);
            var loginWithNewPassword = await service.LoginAsync("bob@example.com", "Changed456", false);

            // Assert
            Assert.False(loginWithOldPassword.IsSuccess);
            Assert.True(loginWithNewPassword.IsSuccess);
        }

        // ------------------- НОВІ ТЕСТИ ДЛЯ "ЗАПАМ'ЯТАТИ МЕНЕ" -------------------

        [Fact]
        public async Task LoginAsync_RememberMeTrue_GeneratesToken()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            await service.RegisterAsync("User", "user@example.com", "pass123");

            // Act
            var result = await service.LoginAsync("user@example.com", "pass123", true);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.User);
            Assert.NotNull(result.RememberMeToken);
            Assert.Equal(result.RememberMeToken, result.User.RememberMeToken);
            Assert.NotNull(result.User.RememberMeTokenExpiry);
            Assert.True(result.User.RememberMeTokenExpiry > DateTime.UtcNow.AddDays(29)); // Більше 29 днів
        }

        [Fact]
        public async Task LoginAsync_RememberMeFalse_NullifiesToken()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            var user = await service.RegisterAsync("User", "user@example.com", "pass123");

            // Спочатку логінимось з "Remember Me", щоб отримати токен
            await service.LoginAsync("user@example.com", "pass123", true);
            Assert.NotNull(user.RememberMeToken); // Переконуємось, що токен є

            // Act
            // Тепер логінимось без "Remember Me"
            var result = await service.LoginAsync("user@example.com", "pass123", false);

            var updatedUser = await context.Users.FindAsync(user.UserId);


            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.RememberMeToken); // Результат не повертає токен
            Assert.Null(updatedUser.RememberMeToken); // Токен видалено з БД
            Assert.Null(updatedUser.RememberMeTokenExpiry); // Дата видалена з БД
        }

        [Fact]
        public async Task LoginWithTokenAsync_ValidToken_ReturnsUser()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            await service.RegisterAsync("User", "user@example.com", "pass123");
            var loginResult = await service.LoginAsync("user@example.com", "pass123", true);
            var token = loginResult.RememberMeToken;

            // Act
            var user = await service.LoginWithTokenAsync(token);

            // Assert
            Assert.NotNull(user);
            Assert.Equal("user@example.com", user.Email);
        }

        [Fact]
        public async Task LoginWithTokenAsync_InvalidToken_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            await service.RegisterAsync("User", "user@example.com", "pass123");
            await service.LoginAsync("user@example.com", "pass123", true);

            // Act
            var user = await service.LoginWithTokenAsync("це_недійсний_токен");

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task LoginWithTokenAsync_ExpiredToken_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);
            var user = await service.RegisterAsync("User", "user@example.com", "pass123");

            // Створюємо прострочений токен вручну
            var expiredToken = "expired_token";
            user.RememberMeToken = expiredToken;
            user.RememberMeTokenExpiry = DateTime.UtcNow.AddMinutes(-5); // Прострочено 5 хвилин тому
            await context.SaveChangesAsync();

            // Act
            var resultUser = await service.LoginWithTokenAsync(expiredToken);

            // Assert
            Assert.Null(resultUser);
        }
    }
}

