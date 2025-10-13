using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System;

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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = HashPassword("password123"),
                Name = "TestUser"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AuthService(context);

            // Act
            var result = await service.LoginAsync("test@example.com", "password123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new User
            {
               Name = "TestUser",
               Email = "test@example.com",
               PasswordHash = HashPassword("password123"),
               Settings = new Settings { Currency = "USD", NotificationEnabled = true }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new AuthService(context);

            // Act
            var result = await service.LoginAsync("test@example.com", "wrongpass");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var result = await service.LoginAsync("notfound@example.com", "pass");

            Assert.Null(result);
        }
    }
}
