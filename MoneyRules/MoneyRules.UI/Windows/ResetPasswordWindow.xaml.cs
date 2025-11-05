using System.Windows;
using MoneyRules.Application.Interfaces;

namespace MoneyRules.UI.Windows
{
    public partial class ResetPasswordWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly string _email;

        public ResetPasswordWindow(IAuthService authService, string email)
        {
            InitializeComponent();
            _authService = authService;
            _email = email;
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string newPass = NewPasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Заповніть усі поля!");
                return;
            }

            if (newPass != confirm)
            {
                MessageBox.Show("Паролі не збігаються!");
                return;
            }

            bool success = await _authService.ResetPasswordAsync(_email, newPass);
            if (success)
            {
                MessageBox.Show("Пароль успішно змінено!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Користувача не знайдено.");
            }
        }
    }
}
