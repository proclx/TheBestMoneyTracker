using System;
using System.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
// Додайте це, якщо ваш LoginResult в DTOs
using MoneyRules.Application.DTOs;

namespace MoneyRules.UI.Windows
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly User _currentUser;

        public ChangePasswordWindow(IAuthService authService, User currentUser)
        {
            InitializeComponent();
            _authService = authService;
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var oldPassword = OldPasswordBox.Password;
            var newPassword = NewPasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Новий пароль і підтвердження не збігаються.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // --- ЗМІНЕНО ТУТ ---
                // 1. Додано 'false' для параметра rememberMe
                // 2. Змінено 'loggedUser' на 'loginResult'
                var loginResult = await _authService.LoginAsync(_currentUser.Email, oldPassword, false);

                // 3. Перевіряємо 'loginResult.IsSuccess' замість 'loggedUser == null'
                if (!loginResult.IsSuccess)
                {
                    MessageBox.Show("Старий пароль невірний.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // --- КІНЕЦЬ ЗМІН ---

                // Змінюємо пароль
                await _authService.ChangePasswordAsync(_currentUser, newPassword);
                MessageBox.Show("Пароль успішно змінено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при зміні пароля: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
