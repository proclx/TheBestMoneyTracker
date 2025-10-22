//using System;
//using System.Threading.Tasks;
//using Xunit;
//using Microsoft.EntityFrameworkCore;
//using MoneyRules.Application.Services;
//using MoneyRules.Infrastructure.Persistence;
//using MoneyRules.Domain.Entities;


//namespace MoneyRules.Tests.Services
//{
//    public class RegistrationServiceTests
//    {
//        private AppDbContext GetInMemoryDbContext()
//        {
//            var options = new DbContextOptionsBuilder<AppDbContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .Options;
//            return new AppDbContext(options);
//        }

//        [Fact]
//        public async Task RegisterAsync_NewUser_SavesAndReturnsUser()
//        {
//            // Arrange
//            var context = GetInMemoryDbContext();
//            var service = new RegistrationService(context);

//            // Act
//            var user = await service.RegisterAsync("John Doe", "john@example.com", "12345");

//            // Assert
//            Assert.NotNull(user);
//            Assert.Equal("john@example.com", user.Email);
//            Assert.NotNull(user.Settings);
//            Assert.Equal("USD", user.Settings.Currency);
//        }

//        [Fact]
//        public async Task RegisterAsync_DuplicateEmail_ThrowsException()
//        {
//            // Arrange
//            var context = GetInMemoryDbContext();
//            var existing = new User { Name = "Existing", Email = "existing@example.com", PasswordHash = "123" };
//            context.Users.Add(existing);
//            await context.SaveChangesAsync();

//            var service = new RegistrationService(context);

//            // Act + Assert
//            await Assert.ThrowsAsync<Exception>(async () =>
//                await service.RegisterAsync("New", "existing@example.com", "pwd"));
//        }

//        [Fact]
//        public async Task LoginAsync_ValidCredentials_ReturnsUser()
//        {
//            // Arrange
//            var context = GetInMemoryDbContext();
//            var service = new RegistrationService(context);

//            var regUser = await service.RegisterAsync("Mary", "mary@example.com", "12345");

//            // Act
//            var result = await service.LoginAsync("mary@example.com", "12345");

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal("mary@example.com", result.Email);
//        }

//        [Fact]
//        public async Task LoginAsync_InvalidPassword_ReturnsNull()
//        {
//            // Arrange
//            var context = GetInMemoryDbContext();
//            var service = new RegistrationService(context);

//            await service.RegisterAsync("Tom", "tom@example.com", "correctpass");

//            // Act
//            var result = await service.LoginAsync("tom@example.com", "wrongpass");

//            // Assert
//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task LoginAsync_UserNotExists_ReturnsNull()
//        {
//            var context = GetInMemoryDbContext();
//            var service = new RegistrationService(context);

//            var result = await service.LoginAsync("unknown@example.com", "12345");

//            Assert.Null(result);
//        }
//    }
//}
