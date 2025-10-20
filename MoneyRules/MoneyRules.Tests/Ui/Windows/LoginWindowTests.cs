using Xunit;
using Moq;
using MoneyRules.UI.Windows;
using MoneyRules.Application.Services;
using System.Threading.Tasks;
using System.Windows;

namespace MoneyRules.Tests
{
    public class LoginWindowTests
    {
        private readonly Mock<IAuthService> _authServiceMock;

        public LoginWindowTests()
        {
            _authServiceMock = new Mock<IAuthService>();
        }

        [WpfFact] // Використовується для тестів, які взаємодіють із WPF
        public async Task LoginButton_Click_ShouldShowSuccessMessage_WhenLoginSucceeds()
        {
            // Arrange
            var fakeUser = new { Name = "Софія" };
            _authServiceMock.Setup(s => s.LoginAsync("test@example.com", "1234"))
                            .ReturnsAsync(fakeUser);

            var window = new LoginWindow(_authServiceMock.Object);

            // Імітуємо поля введення
            window.EmailTextBox = new System.Windows.Controls.TextBox { Text = "test@example.com" };
            window.PasswordBox = new System.Windows.Controls.PasswordBox();
            window.PasswordBox.Password = "1234";

            bool messageShown = false;
            MessageBoxManager.OverrideMessageBox((text, caption, button, icon) =>
            {
                messageShown = true;
                return MessageBoxResult.OK;
            });

            // Act
            var method = window.GetType().GetMethod("LoginButton_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(window, new object[] { null, new RoutedEventArgs() });
            await Task.Delay(100); // дочекатись асинхронної дії

            // Assert
            Assert.True(messageShown);
        }

        [WpfFact]
        public async Task LoginButton_Click_ShouldShowErrorMessage_WhenLoginFails()
        {
            // Arrange
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync((object?)null);

            var window = new LoginWindow(_authServiceMock.Object);
            window.EmailTextBox = new System.Windows.Controls.TextBox { Text = "wrong@example.com" };
            window.PasswordBox = new System.Windows.Controls.PasswordBox { Password = "wrong" };

            bool errorShown = false;
            MessageBoxManager.OverrideMessageBox((text, caption, button, icon) =>
            {
                errorShown = caption == "Помилка";
                return MessageBoxResult.OK;
            });

            // Act
            var method = window.GetType().GetMethod("LoginButton_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(window, new object[] { null, new RoutedEventArgs() });
            await Task.Delay(100);

            // Assert
            Assert.True(errorShown);
        }

        [WpfFact]
        public async Task LoginButton_Click_ShouldHandleExceptionGracefully()
        {
            // Arrange
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                            .ThrowsAsync(new System.Exception("DB connection failed"));

            var window = new LoginWindow(_authServiceMock.Object);
            window.EmailTextBox = new System.Windows.Controls.TextBox { Text = "test@example.com" };
            window.PasswordBox = new System.Windows.Controls.PasswordBox { Password = "1234" };

            bool errorMessageShown = false;
            MessageBoxManager.OverrideMessageBox((text, caption, button, icon) =>
            {
                errorMessageShown = text.Contains("Сталася помилка");
                return MessageBoxResult.OK;
            });

            // Act
            var method = window.GetType().GetMethod("LoginButton_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(window, new object[] { null, new RoutedEventArgs() });
            await Task.Delay(100);

            // Assert
            Assert.True(errorMessageShown);
        }
    }

    // Допоміжний клас для перехоплення MessageBox (імітація)
    public static class MessageBoxManager
    {
        public static Func<string, string, MessageBoxButton, MessageBoxImage, MessageBoxResult>? MessageBoxOverride;

        public static void OverrideMessageBox(Func<string, string, MessageBoxButton, MessageBoxImage, MessageBoxResult> fake)
        {
            MessageBoxOverride = fake;
        }

        public static MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            if (MessageBoxOverride != null)
                return MessageBoxOverride(message, caption, button, icon);

            return MessageBox.Show(message, caption, button, icon);
        }
    }

    // Кастомний атрибут для тестів, що взаємодіють із WPF
    public class WpfFactAttribute : FactAttribute { }
}
