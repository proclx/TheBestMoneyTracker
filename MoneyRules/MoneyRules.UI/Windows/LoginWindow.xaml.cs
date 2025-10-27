using System.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;

namespace MoneyRules.UI.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly ITransactionService _transactionService;
        private readonly IUserProfileService _profileService;

        public LoginWindow(IAuthService authService, ITransactionService transactionService, IUserProfileService profileService)
        {
            InitializeComponent();
            _authService = authService;
            _transactionService = transactionService;
            _profileService = profileService;
        }


        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            try
            {
                var user = await _authService.LoginAsync(email, password);

                if (user != null)
                {
                    // Зберігаємо поточного користувача
                    System.Windows.Application.Current.Properties["CurrentUser"] = user;

                    // Відкриваємо MainWindow із сервісами
                    var mainWindow = new MainWindow(_transactionService, _authService, _profileService);
                    mainWindow.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Невірний email або пароль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка при вході", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Повертаємо користувача на WelcomeWindow
            var welcomeWindow = new WelcomeWindow(_authService, _transactionService, _profileService);
            welcomeWindow.Show();
            this.Close();
        }
    }
}


