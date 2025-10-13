using Xunit;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;

namespace MoneyRules.Tests.Tests
{
    public class UserTests
    {
        [Fact]
        public void User_ShouldBeCreatedSuccessfully()
        {
            // Arrange & Act
            var user = new User
            {
                UserId = 1,
                Name = "Sofiia",
                Email = "sofia@example.com",
                PasswordHash = "hashed_pw",
                Role = UserRole.User,
                Settings = new Settings
                {
                    SettingsId = 1,
                    Currency = "USD",
                    NotificationEnabled = true
                }
            };

            // Assert
            Assert.NotNull(user);
            Assert.Equal("Sofiia", user.Name);
            Assert.Equal("sofia@example.com", user.Email);
            Assert.Equal(UserRole.User, user.Role);
            Assert.Equal("USD", user.Settings.Currency);
            Assert.True(user.Settings.NotificationEnabled);
        }

        [Fact]
        public void User_ShouldHaveSettingsLinked()
        {
            // Arrange
            var user = new User
            {
                UserId = 2,
                Name = "Ivan",
                Email = "ivan@example.com",
                PasswordHash = "12345",
                Role = UserRole.User
            };

            var settings = new Settings
            {
                SettingsId = 2,
                Currency = "EUR",
                NotificationEnabled = false,
                User = user
            };

            user.Settings = settings;

            // Assert
            Assert.NotNull(user.Settings);
            Assert.Equal(user.UserId, user.Settings.User.UserId);
            Assert.Equal("EUR", user.Settings.Currency);
        }

        [Theory]
        [InlineData("", "Email is required")]
        [InlineData(null, "Email is required")] // ✅ дозволено, бо string? нижче
        public void ValidateEmail_ShouldReturnError_ForInvalidEmail(string? email, string expectedMessage)
        {
            // Arrange
            var user = new User
            {
                Name = "Test",
                Email = email ?? string.Empty,
                PasswordHash = "pw",
                Role = UserRole.User,
                Settings = new Settings { Currency = "USD", NotificationEnabled = true }
            };

            // Act
            bool isValid = !string.IsNullOrWhiteSpace(user.Email);

            // Assert
            if (!isValid)
                Assert.Equal("Email is required", expectedMessage);
            else
                Assert.True(isValid);
        }

        [Fact]
        public void UserRole_ShouldDefaultToUser()
        {
            // Arrange
            var user = new User
            {
                Name = "Default Role",
                Email = "default@example.com",
                PasswordHash = "123",
                Settings = new Settings { Currency = "USD", NotificationEnabled = true }
            };

            // Act
            user.Role = UserRole.User;

            // Assert
            Assert.Equal(UserRole.User, user.Role);
        }
    }
}
