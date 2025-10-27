using Xunit;
using MoneyRules.Application.Services;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MoneyRules.Tests.Tests
{
    public class UserProfileServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void UpdateUser_ShouldSaveUserAndSettings()
        {
            using var context = GetInMemoryDbContext();
            var service = new UserProfileService(context);

            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                ProfilePhoto = Array.Empty<byte>(),
                PasswordHash = "dummyhash",
                Settings = new Settings
                {
                    Currency = "USD",
                    NotificationEnabled = true
                }
            };

            service.UpdateUser(user);

            var savedUser = context.Users.Include(u => u.Settings).FirstOrDefault();
            Assert.NotNull(savedUser);
            Assert.Equal("Test User", savedUser.Name);
            Assert.Equal("USD", savedUser.Settings.Currency);
            Assert.True(savedUser.Settings.NotificationEnabled);
        }

        [Fact]
        public void ChangeProfilePhoto_ShouldUpdatePhoto()
        {
            using var context = GetInMemoryDbContext();
            var service = new UserProfileService(context);

            var user = new User
            {
                Name = "Photo User",
                Email = "photo@example.com",
                ProfilePhoto = Array.Empty<byte>(),
                PasswordHash = "dummyhash",
            };

            service.UpdateUser(user);

            byte[] photoData = new byte[] { 1, 2, 3, 4, 5 };
            service.ChangeProfilePhoto(user, photoData);

            var savedUser = context.Users.FirstOrDefault();
            Assert.NotNull(savedUser.ProfilePhoto);
            Assert.Equal(photoData, savedUser.ProfilePhoto);
        }

        [Fact]
        public void GetUserById_ShouldReturnUserWithSettings()
        {
            using var context = GetInMemoryDbContext();
            var service = new UserProfileService(context);

            var user = new User
            {
                Name = "Find Me",
                Email = "findme@example.com",
                ProfilePhoto = Array.Empty<byte>(),
                PasswordHash = "dummyhash",
                Settings = new Settings { Currency = "EUR" }
            };
            context.Users.Add(user);
            context.SaveChanges();

            var fetchedUser = service.GetUserById(user.UserId);
            Assert.NotNull(fetchedUser);
            Assert.Equal("Find Me", fetchedUser.Name);
            Assert.NotNull(fetchedUser.Settings);
            Assert.Equal("EUR", fetchedUser.Settings.Currency);
        }
    }
}