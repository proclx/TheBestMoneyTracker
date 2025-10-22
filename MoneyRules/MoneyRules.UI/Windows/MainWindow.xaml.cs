using System;
using System.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.Application.Services;
using MoneyRules.Infrastructure.Persistence;

namespace MoneyRules.UI.Windows
{
    public partial class MainWindow : Window
    {
        private readonly ITransactionService? _transactionService;
        private readonly IAuthService? _authService;
        private readonly IUserProfileService? _profileService;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(ITransactionService transactionService, IAuthService authService, IUserProfileService profileService)
        {
            InitializeComponent();

            _transactionService = transactionService;
            _authService = authService;
            _profileService = profileService;
        }

        // 📁 Додати транзакцію
        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Додати транзакцію натиснуто!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 📜 Переглянути історію
        private void OpenHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Перегляд історії натиснуто!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 💰 Встановити ліміт
        private void BtnSetMonthlyLimit_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Встановлення ліміту натиснуто!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 💡 Оновити поради
        private void RefreshAdviceButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Оновлення порад натиснуто!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 🖼️ Змінити фото профілю
        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Зміна фото натиснута!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 💾 Зберегти профіль
        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Профіль збережено!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 🚪 Вийти з акаунту
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ви дійсно хочете вийти з акаунту?",
                                         "Підтвердження виходу",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Ви вийшли з акаунту.", "Вихід", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }
    }
}
