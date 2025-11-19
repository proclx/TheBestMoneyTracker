using System;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.UI.Utils;

namespace MoneyRules.UI.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly ITransactionService _transactionService;
        private readonly IUserProfileService _profileService;
        private readonly IAdviceService _adviceService;
        private readonly ICurrencyService _currencyService;

        private string _currentEmail;
        private string _currentPassword;

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
            _currentEmail = EmailTextBox.Text.Trim();
            _currentPassword = PasswordBox.Password;
            bool rememberMe = RememberMeCheckBox.IsChecked == true;

            if (string.IsNullOrWhiteSpace(_currentEmail) || string.IsNullOrWhiteSpace(_currentPassword))
            {
                MessageBox.Show("Email та пароль не можуть бути порожніми.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = await _authService.LoginAsync(_currentEmail, _currentPassword, rememberMe);
            if (result.IsSuccess && result.User != null)
            {
                // Генеруємо 2FA код
                string code = _authService.GenerateTwoFactorCode(_currentEmail);
                MessageBox.Show($"Ваш код 2FA: {code}", "Двофакторна автентифікація", MessageBoxButton.OK, MessageBoxImage.Information);

                // Показуємо панель для введення коду 2FA
                TwoFactorPanel.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Невірний email або пароль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var welcomeWindow = (App.Current as App)?.ServiceProvider?.GetRequiredService<WelcomeWindow>()
                ?? new WelcomeWindow(_authService, _transactionService, _profileService, _adviceService, _currencyService);
            welcomeWindow.Show();
            this.Close();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            var window = new ForgotPasswordWindow(_authService);
            window.ShowDialog();
        }

        private async void ConfirmTwoFactorButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = TwoFactorCodeTextBox.Text.Trim();
            if (_authService.VerifyTwoFactorCode(_currentEmail, enteredCode))
            {
                _authService.RemoveTwoFactorCode(_currentEmail);

                // Використовуємо асинхронний виклик для логіну
                var loginResult = await _authService.LoginAsync(_currentEmail, _currentPassword, RememberMeCheckBox.IsChecked == true);
                if (loginResult.IsSuccess && loginResult.User != null)
                {
                    // Зберігаємо поточного користувача
                    System.Windows.Application.Current.Properties["CurrentUser"] = loginResult.User;

                    // Зберігаємо токен "Remember Me" якщо обрано
                    if (RememberMeCheckBox.IsChecked == true && !string.IsNullOrEmpty(loginResult.RememberMeToken))
                    {
                        SecureTokenStorage.SaveToken(loginResult.RememberMeToken);
                    }

                    var mainWindow = (App.Current as App)?.ServiceProvider?.GetRequiredService<MainWindow>()
                        ?? throw new InvalidOperationException("Could not create MainWindow");

                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Помилка входу після підтвердження 2FA.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Невірний код 2FA.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

