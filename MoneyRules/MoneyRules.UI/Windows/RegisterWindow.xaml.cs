using System;
using System.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;

namespace MoneyRules.UI.Windows
{
    public partial class RegisterWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly ITransactionService _transactionService;

        public RegisterWindow(IAuthService authService, ITransactionService transactionService)
        {
            InitializeComponent();
            _authService = authService;
            _transactionService = transactionService;
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = NameBox.Text;
                var email = EmailBox.Text;
                var password = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Будь ласка, заповніть усі поля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Реєстрація користувача
                var user = await _authService.RegisterAsync(name, email, password);
                MessageBox.Show($"Користувач {user.Name} зареєстрований успішно!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                // Логіка переходу після реєстрації
                if (LoginAfterRegisterCheckBox.IsChecked == true)
                {
                    System.Windows.Application.Current.Properties["CurrentUser"] = user;

                    // Відкриваємо MainWindow
                    var mainWindow = new MainWindow(_transactionService, _authService);
                    mainWindow.Show();
                }
                else
                {
                    // Повертаємо на WelcomeWindow
                    var welcomeWindow = new WelcomeWindow(_authService, _transactionService);
                    welcomeWindow.Show();
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Повертаємо на WelcomeWindow
            var welcomeWindow = new WelcomeWindow(_authService, _transactionService);
            welcomeWindow.Show();
            this.Close();
        }
    }
}



