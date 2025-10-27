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
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            // Act
            var user = await service.RegisterAsync("John Doe", "john@example.com", "123456");

            // Assert
            Assert.NotNull(user);
            Assert.Equal("john@example.com", user.Email);
            Assert.NotNull(user.Settings);
            Assert.Equal("USD", user.Settings.Currency);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            var registeredUser = await service.RegisterAsync("Mary", "mary@example.com", "123456");

            // Act
            var result = await service.LoginAsync("mary@example.com", "123456");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("mary@example.com", result.Email);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            await service.RegisterAsync("Tom", "tom@example.com", "correctpass");

            // Act
            var result = await service.LoginAsync("tom@example.com", "wrongpass");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_UserNotExists_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context);

            // Act
            var result = await service.LoginAsync("unknown@example.com", "123456");

            // Assert
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
    }
}
