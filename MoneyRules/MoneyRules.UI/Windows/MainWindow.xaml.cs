using System.Windows;
using MoneyRules.Application.Services;
using MoneyRules.UI.Windows;
using MoneyRules.Infrastructure.Persistence;
using System.Windows.Controls;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace MoneyRules.UI
{
    public partial class MainWindow : Window
    {
        private readonly ITransactionService _transactionService;
        private readonly IAdviceService _adviceService;
        private readonly AppDbContext _context = new AppDbContext();
        private User _currentUser;

        public MainWindow(ITransactionService transactionService)
        {
            InitializeComponent();

            _transactionService = transactionService;
            _adviceService = new AdviceService();

            LoadAdvice();
            LoadUserProfile();
        }
        private void OpenHistory_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо поточного користувача, який залогінився
            if (System.Windows.Application.Current.Properties["CurrentUser"] is not Domain.Entities.User currentUser)
            {
                MessageBox.Show("Користувач не знайдений. Увійдіть знову.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Створюємо вікно історії і передаємо сервіс + ідентифікатор користувача
            var historyWindow = new TransactionHistoryWindow(_transactionService, currentUser.UserId);
            historyWindow.ShowDialog();
        }
        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddTransactionWindow
            {
                Owner = this
            };

            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                MessageBox.Show("Транзакція успішно додана!");
                // refresh advice when a new transaction is added
                LoadAdvice();
            }
        }
        private void LoadAdvice()
        {
            try
            {
                using var db = new AppDbContext();
                var transactions = db.Transactions
                    .OrderByDescending(t => t.Date)
                    .ToList();

                var tips = _adviceService.GetAdvice(transactions);
                AdviceList.ItemsSource = tips.Select(t => new TextBlock { Text = t, TextWrapping = TextWrapping.Wrap });
            }
            catch (System.Exception ex)
            {
                AdviceList.ItemsSource = new[] { new TextBlock { Text = "Не вдалося отримати поради: " + ex.Message } };
            }
        }
        private void RefreshAdviceButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAdvice();
        }
        private void LoadUserProfile()
        {
            _currentUser = (User)System.Windows.Application.Current.Properties["CurrentUser"];

            if (_currentUser == null)
            {
                MessageBox.Show("No user logged in.");
                return;
            }

            // Basic info
            TxtName.Text = _currentUser.Name;
            TxtEmail.Text = _currentUser.Email;

            // Profile photo
            if (_currentUser.ProfilePhoto != null && _currentUser.ProfilePhoto.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(_currentUser.ProfilePhoto))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        ProfileImage.Source = bitmap;
                    }
                }
                catch
                {
                    // In case the stored data is invalid (corrupted image bytes)
                    ProfileImage.Source = null;
                }
            }

            // Settings
            if (_currentUser.Settings != null)
            {
                foreach (ComboBoxItem item in CmbCurrency.Items)
                {
                    if (item.Content.ToString() == _currentUser.Settings.Currency)
                    {
                        CmbCurrency.SelectedItem = item;
                        break;
                    }
                }

                ChkNotifications.IsChecked = _currentUser.Settings.NotificationEnabled;
            }
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Image Files|*.jpg;*.png;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                byte[] imageData = File.ReadAllBytes(dlg.FileName);
                _currentUser.ProfilePhoto = imageData;

                using (var ms = new MemoryStream(imageData))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    ProfileImage.Source = bitmap;
                }
            }
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User not loaded.");
                return;
            }

            // Update user info
            _currentUser.Name = TxtName.Text;
            _currentUser.Email = TxtEmail.Text;

            // Update settings
            var selectedCurrency = (CmbCurrency.SelectedItem as ComboBoxItem)?.Content.ToString()
                        ?? _currentUser.Settings?.Currency
                        ?? "UAH";
            if (_currentUser.Settings == null)
                _currentUser.Settings = new Settings { UserId = _currentUser.UserId };

            _currentUser.Settings.Currency = selectedCurrency;
            _currentUser.Settings.NotificationEnabled = ChkNotifications.IsChecked ?? false;

            // Save to DB
            _context.Users.Update(_currentUser);
            _context.SaveChanges();

            // Update the global app property
            System.Windows.Application.Current.Properties["CurrentUser"] = _currentUser;

            MessageBox.Show("Profile updated successfully.");
        }
    }
}
