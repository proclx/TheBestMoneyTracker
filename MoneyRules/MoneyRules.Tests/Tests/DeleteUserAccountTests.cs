using System.Threading.Tasks;
using Xunit;
using MoneyRules.Application.Services;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;

namespace MoneyRules.Tests
{
    public class DeleteUserAccountTests
    {
        private async Task<AppDbContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
                .Options;

            var context = new AppDbContext(options);

            // Додаємо тестового користувача
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = new AuthService(context).HashPassword("Password123"),
                Role = UserRole.User,
                Settings = new Settings
                {
                    Currency = "USD",
                    NotificationEnabled = true
                }
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task DeleteUserAccountAsync_ShouldDeleteUser_WhenCredentialsAreCorrect()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var authService = new AuthService(context);

            // Act
            bool result = await authService.DeleteUserAccountAsync("test@example.com", "Password123");

            // Assert
            Assert.True(result); // Повертає true
            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.Null(userInDb); // Користувач видалений
        }

        [Fact]
        public async Task DeleteUserAccountAsync_ShouldFail_WhenEmailIsIncorrect()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var authService = new AuthService(context);

            // Act
            bool result = await authService.DeleteUserAccountAsync("wrong@example.com", "Password123");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUserAccountAsync_ShouldFail_WhenPasswordIsIncorrect()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var authService = new AuthService(context);

            // Act
            bool result = await authService.DeleteUserAccountAsync("test@example.com", "WrongPassword");

            // Assert
            Assert.False(result);
        }
    }
}

