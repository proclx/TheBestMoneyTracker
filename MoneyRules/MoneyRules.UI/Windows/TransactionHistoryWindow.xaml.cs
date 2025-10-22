using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using System.Windows;

namespace MoneyRules.UI.Windows
{
    public partial class TransactionHistoryWindow : Window
    {
        private readonly ITransactionService _transactionService;
        private readonly int _currentUserId;
        private Transaction _selectedTransaction;

        public TransactionHistoryWindow(ITransactionService transactionService, int currentUserId)
        {
            InitializeComponent();
            _transactionService = transactionService;
            _currentUserId = currentUserId;
            LoadDataAsync();
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

        private void TransactionGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedTransaction = TransactionGrid.SelectedItem as Transaction;
        }

        private async void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTransaction == null)
            {
                MessageBox.Show("Виберіть транзакцію для редагування.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var categories = await _transactionService.GetUserCategoriesAsync(_currentUserId);

            var editWindow = new EditTransactionWindow(_transactionService, _selectedTransaction, categories);
            editWindow.ShowDialog();

            await LoadDataAsync();
        }
    }
}
