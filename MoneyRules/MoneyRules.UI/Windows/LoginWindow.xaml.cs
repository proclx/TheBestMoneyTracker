using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.UI.Utils; // <-- Додайте це

namespace MoneyRules.UI.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly ITransactionService _transactionService;
        private readonly IUserProfileService _profileService; // Припустимо, що у вас є ці сервіси
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

        // --- ОНОВЛЕНИЙ МЕТОД ---
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            bool rememberMe = RememberMeCheckBox.IsChecked == true; // <-- Зчитуємо CheckBox

            try
            {
                // Використовуємо новий метод сервісу
                var result = await _authService.LoginAsync(email, password, rememberMe);

                if (result.IsSuccess && result.User != null)
                {
                    // --- НОВА ЛОГІКА ЗБЕРЕЖЕННЯ ТОКЕНА ---
                    if (rememberMe && !string.IsNullOrEmpty(result.RememberMeToken))
                    {
                        SecureTokenStorage.SaveToken(result.RememberMeToken);
                    }
                    else
                    {
                        // Якщо "Remember Me" не обрано, чистимо старий токен
                        SecureTokenStorage.ClearToken();
                    }
                    // --- КІНЕЦЬ НОВОЇ ЛОГІКИ ---

                    // Зберігаємо поточного користувача
                    System.Windows.Application.Current.Properties["CurrentUser"] = result.User;

                    // Відкриваємо MainWindow із сервісами
                    var mainWindow = (App.Current as App)?.ServiceProvider?.GetRequiredService<MainWindow>()
                        ?? throw new InvalidOperationException("Could not create MainWindow");
                    mainWindow.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage ?? "Невірний email або пароль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var welcomeWindow = (App.Current as App)?.ServiceProvider?.GetRequiredService<WelcomeWindow>()
                ?? new WelcomeWindow(_authService, _transactionService, _profileService, _adviceService, _currencyService); // Fallback
            welcomeWindow.Show();
            this.Close();
        }
        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            var window = new ForgotPasswordWindow(_authService);
            window.ShowDialog();
        }

    }
}
