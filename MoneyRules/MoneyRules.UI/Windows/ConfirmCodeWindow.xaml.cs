using System.Windows;
using MoneyRules.Application.Interfaces;

namespace MoneyRules.UI.Windows
{
    public partial class ConfirmCodeWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly string _email;

        public ConfirmCodeWindow(IAuthService authService, string email)
        {
            InitializeComponent();
            _authService = authService;
            _email = email;
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string inputCode = CodeTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(inputCode))
            {
                MessageBox.Show("Введіть код підтвердження!");
                return;
            }

            bool valid = await _authService.VerifyConfirmationCodeAsync(_email, inputCode);

            if (!valid)
            {
                MessageBox.Show("Невірний код підтвердження!");
                return;
            }

            // Якщо код правильний — відкриваємо вікно для нового паролю
            var resetWindow = new ResetPasswordWindow(_authService, _email);
            resetWindow.Show();
            this.Close();
        }
    }
}

