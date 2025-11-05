using System;
using System.Threading.Tasks;
using System.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.Application.Services;

namespace MoneyRules.UI.Windows
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly IAuthService _authService;

        public ForgotPasswordWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void SendCode_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Введіть email!");
                return;
            }

            bool exists = await _authService.CheckEmailExistsAsync(email);
            if (!exists)
            {
                MessageBox.Show("Користувача з таким email не знайдено!");
                return;
            }

            string code = _authService.GenerateConfirmationCode(email);

            // Імітація “надсилання” коду
            MessageBox.Show($"Імітація розсилки: код підтвердження — {code}");

            // Відкрити вікно підтвердження
            var confirmWindow = new ConfirmCodeWindow(_authService, email);
            confirmWindow.Show();
            this.Close();
        }
    }
}
