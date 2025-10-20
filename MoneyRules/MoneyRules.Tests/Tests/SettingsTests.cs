using MoneyRules.Domain.Entities;
using Xunit;

namespace MoneyRules.Tests.Tests
{
    public class SettingsTests
    {
        [Fact]
        public void Settings_ShouldInitializeWithDefaultValues()
        {
            var settings = new Settings
            {
                SettingsId = 1,
                Currency = "USD",
                NotificationEnabled = true,
                UserId = 99
            };

            Assert.Equal(1, settings.SettingsId);
            Assert.Equal("USD", settings.Currency);
            Assert.True(settings.NotificationEnabled);
            Assert.Equal(99, settings.UserId);
        }

        [Fact]
        public void Settings_ShouldReferenceUser()
        {
            var user = new User { Name = "Sofia" };
            var settings = new Settings { User = user };

            Assert.Equal("Sofia", settings.User.Name);
        }
    }
}
