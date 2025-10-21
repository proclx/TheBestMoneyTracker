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
    }
}
