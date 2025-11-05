using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Application.Services;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;

namespace MoneyRules.Tests.Services
{
    public class AuthServicePasswordRecoveryTests
    {
        private async Task<AppDbContext> GetDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            var user = new User
            {
                Name = "TestUser",
                Email = "test@example.com",
                PasswordHash = "old_hash",
                Role = UserRole.User
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task CheckEmailExistsAsync_ShouldReturnTrue_IfEmailExists()
        {
            var context = await GetDbContextAsync();
            var service = new AuthService(context);

            var result = await service.CheckEmailExistsAsync("test@example.com");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckEmailExistsAsync_ShouldReturnFalse_IfEmailNotExists()
        {
            var context = await GetDbContextAsync();
            var service = new AuthService(context);

            var result = await service.CheckEmailExistsAsync("noone@example.com");

            Assert.False(result);
        }

        [Fact]
        public void GenerateConfirmationCode_ShouldReturnSixDigitCode()
        {
            var context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("CodeTest").Options);

            var service = new AuthService(context);

            var code = service.GenerateConfirmationCode("test@example.com");

            Assert.Matches(@"^\d{6}$", code);
        }

        [Fact]
        public async Task VerifyConfirmationCodeAsync_ShouldReturnTrue_WhenCodeIsCorrect()
        {
            var context = await GetDbContextAsync();
            var service = new AuthService(context);
            var email = "test@example.com";

            var code = service.GenerateConfirmationCode(email);
            var result = await service.VerifyConfirmationCodeAsync(email, code);

            Assert.True(result);
        }

        [Fact]
        public async Task VerifyConfirmationCodeAsync_ShouldReturnFalse_WhenCodeIsWrong()
        {
            var context = await GetDbContextAsync();
            var service = new AuthService(context);
            var email = "test@example.com";

            service.GenerateConfirmationCode(email);
            var result = await service.VerifyConfirmationCodeAsync(email, "123456");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldUpdatePassword_WhenUserExists()
        {
            var context = await GetDbContextAsync();
            var service = new AuthService(context);
            var email = "test@example.com";

            var result = await service.ResetPasswordAsync(email, "newPassword123");

            var user = await context.Users.FirstAsync(u => u.Email == email);

            Assert.True(result);
            Assert.NotEqual("old_hash", user.PasswordHash);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnFalse_WhenUserNotExists()
        {
            var context = await GetDbContextAsync();
            var service = new AuthService(context);

            var result = await service.ResetPasswordAsync("noone@example.com", "newpass");

            Assert.False(result);
        }
    }
}

