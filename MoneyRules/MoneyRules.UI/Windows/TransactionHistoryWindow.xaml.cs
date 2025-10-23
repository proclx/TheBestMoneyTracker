using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MoneyRules.UI.Windows
{
    public partial class TransactionHistoryWindow : Window
    {
        private readonly ITransactionService _transactionService;
        private readonly int _currentUserId;

        public TransactionHistoryWindow(ITransactionService transactionService, int currentUserId)
        {
            InitializeComponent();
            _transactionService = transactionService;
            _currentUserId = currentUserId;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var filter = new TransactionFilter { UserId = _currentUserId };
            var transactions = await _transactionService.GetTransactionsAsync(filter);
            TransactionGrid.ItemsSource = transactions.ToList();
        }

        private async void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var filter = new TransactionFilter
            {
                UserId = _currentUserId,
                FromDate = FromDatePicker.SelectedDate,
                ToDate = ToDatePicker.SelectedDate,
                Type = TypeComboBox.SelectedItem as TransactionType?
            };

            var transactions = await _transactionService.GetTransactionsAsync(filter);
            TransactionGrid.ItemsSource = transactions.ToList();
        }

        private async void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is int transactionId)
            {
                await ConfirmAndDeleteAsync(transactionId);
            }
        }

        private async Task ConfirmAndDeleteAsync(int transactionId)
        {
            var confirm = MessageBox.Show("Ви впевнені, що хочете видалити цю транзакцію?",
                                          "Підтвердження",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                bool success = await _transactionService.DeleteTransactionAsync(transactionId);

                if (success)
                {
                    MessageBox.Show("Транзакцію видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Транзакцію не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
