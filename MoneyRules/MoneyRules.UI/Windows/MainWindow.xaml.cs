using System.Windows;
using MoneyRules.Application.Services;
using MoneyRules.UI.Windows;
using MoneyRules.Infrastructure.Persistence;
using System.Windows.Controls;
using MoneyRules.Application.Interfaces;

namespace MoneyRules.UI
{
    public partial class MainWindow : Window
    {
        private readonly ITransactionService _transactionService;

        public MainWindow(ITransactionService transactionService)
        {
            InitializeComponent();
            _transactionService = transactionService;
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
        private readonly IAdviceService _adviceService;

        public MainWindow()
        {
            InitializeComponent();
            _adviceService = new AdviceService();
            LoadAdvice();
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
    }
}
