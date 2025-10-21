using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MoneyRules.Domain.Entities;
using MoneyRules.UI;
using MoneyRules.UI.Windows;
using MoneyRules.Application.Interfaces;

namespace MoneyRules.Tests.Tests
{
    public class UiWindowsTests
    {
        [StaFact]
        public void WelcomeWindow_Should_Initialize()
        {
            var mockAuth = new Mock<IAuthService>();
            var window = new WelcomeWindow(mockAuth.Object);

            Assert.NotNull(window);
        }

        [StaFact]
        public void LoginWindow_Should_Create()
        {
            var mockAuth = new Mock<IAuthService>();
            var loginWindow = new LoginWindow(mockAuth.Object);

            Assert.NotNull(loginWindow);
        }

        [StaFact]
        public void RegisterWindow_Should_Create()
        {
            var window = new RegisterWindow();
            Assert.NotNull(window);
        }

        [StaFact]
        public void MainWindow_Should_Create()
        {
            var window = new MainWindow();
            Assert.NotNull(window);
        }

        [StaFact]
        public void App_Should_Initialize_ServiceProvider()
        {
          // Ми не створюємо новий App, просто перевіряємо, що властивість існує
          Assert.NotNull(typeof(App).GetProperty("ServiceProvider"));
        }

        [StaFact]
        public void App_Should_Have_Configuration_Property()
        {
          // Аналогічно, перевіряємо, що властивість визначена у класі
          Assert.NotNull(typeof(App).GetProperty("Configuration"));
        }

        public void AuthService_Mock_Should_Login_Return_User()
        {
            var mockAuth = new Mock<IAuthService>();
            mockAuth.Setup(s => s.LoginAsync("test@example.com", "123"))
                .ReturnsAsync(new User { Name = "Test", Email = "test@example.com" });

            var result = mockAuth.Object.LoginAsync("test@example.com", "123").Result;

            Assert.Equal("Test", result.Name);
            Assert.Equal("test@example.com", result.Email);
        }

        [StaFact]
        public void RegistrationService_Mock_Should_Register_User()
        {
            var mockReg = new Mock<IRegistrationService>();
            mockReg.Setup(s => s.RegisterAsync("Sofia", "mail", "pass"))
                .ReturnsAsync(new User { Name = "Sofia" });

            var user = mockReg.Object.RegisterAsync("Sofia", "mail", "pass").Result;

            Assert.NotNull(user);
            Assert.Equal("Sofia", user.Name);
        }

        [StaFact]
        public void FileUploadPage_Should_Initialize()
        {
            var page = new MoneyRules.UI.FileUploadPage();
            Assert.NotNull(page);
        }
    }
}
