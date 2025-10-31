using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;

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
        public async Task LoginAsync_ValidCredentials_ReturnsUser()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var registeredUser = await service.RegisterAsync("Mary", "mary@example.com", "123456");
            var result = await service.LoginAsync("mary@example.com", "123456");

            Assert.NotNull(result);
            Assert.Equal("mary@example.com", result.Email);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            await service.RegisterAsync("Tom", "tom@example.com", "correctpass");
            var result = await service.LoginAsync("tom@example.com", "wrongpass");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_UserNotExists_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var result = await service.LoginAsync("unknown@example.com", "123456");

            Assert.Null(result);
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

        // ------------------- Тести зміни паролю -------------------

        [Fact]
        public async Task ChangePassword_UpdatesPasswordHashSuccessfully()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var user = await service.RegisterAsync("Alice", "alice@example.com", "OldPass123");
            var oldHash = user.PasswordHash;
            var newPassword = "NewPass456";

            // Act
            user.PasswordHash = service.HashPassword(newPassword);
            context.Users.Update(user);
            await context.SaveChangesAsync();

            // Assert
            Assert.NotEqual(oldHash, user.PasswordHash);
            Assert.True(service.VerifyPassword(newPassword, user.PasswordHash));
            Assert.False(service.VerifyPassword("OldPass123", user.PasswordHash));
        }

        [Fact]
        public async Task LoginAfterPasswordChange_WorksWithNewPassword()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var user = await service.RegisterAsync("Bob", "bob@example.com", "Initial123");

            // Confirm login with old password works
            var loginOld = await service.LoginAsync("bob@example.com", "Initial123");
            Assert.NotNull(loginOld);

            // Change password
            user.PasswordHash = service.HashPassword("Changed456");
            context.Users.Update(user);
            await context.SaveChangesAsync();

            // Act
            var loginWithOldPassword = await service.LoginAsync("bob@example.com", "Initial123");
            var loginWithNewPassword = await service.LoginAsync("bob@example.com", "Changed456");

            // Assert
            Assert.Null(loginWithOldPassword);
            Assert.NotNull(loginWithNewPassword);
        }
    }
}

