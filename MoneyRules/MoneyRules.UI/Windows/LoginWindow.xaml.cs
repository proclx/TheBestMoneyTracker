using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;

namespace MoneyRules.UI.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly ITransactionService _transactionService;
        private readonly IUserProfileService _profileService;
        private readonly IAdviceService _adviceService;
        private readonly ICurrencyService _currencyService;

        public LoginWindow(
            IAuthService authService,
            ITransactionService transactionService,
            IUserProfileService profileService,
            IAdviceService adviceService,
            ICurrencyService currencyService)
        {
            InitializeComponent();
            _authService = authService;
            _transactionService = transactionService;
            _profileService = profileService;
            _adviceService = adviceService;
            _currencyService = currencyService;
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
                    var mainWindow = (App.Current as App)?.ServiceProvider?.GetRequiredService<MainWindow>()
                        ?? throw new InvalidOperationException("Could not create MainWindow");
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
            var welcomeWindow = new WelcomeWindow(_authService, _transactionService, _profileService, _adviceService, _currencyService);
            welcomeWindow.Show();
            this.Close();
        }
    }
}


