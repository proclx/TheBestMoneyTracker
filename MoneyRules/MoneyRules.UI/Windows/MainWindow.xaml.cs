using System.Windows;
using MoneyRules.Application.Services;
using MoneyRules.UI.Windows;
using WpfApp = System.Windows.Application;

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
            if (WpfApp.Current.Properties["CurrentUser"] is not Domain.Entities.User currentUser)
            {
                MessageBox.Show("Користувач не знайдений. Увійдіть знову.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Створюємо вікно історії і передаємо сервіс + ідентифікатор користувача
            var historyWindow = new TransactionHistoryWindow(_transactionService, currentUser.UserId);
            historyWindow.ShowDialog();
        }
    }
}
