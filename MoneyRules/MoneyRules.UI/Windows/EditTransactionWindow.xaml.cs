using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MoneyRules.UI.Windows
{
    public partial class EditTransactionWindow : Window
    {
        private readonly ITransactionService _transactionService;
        private readonly Transaction _transaction;
        private readonly List<Category> _categories;

        public EditTransactionWindow(ITransactionService transactionService, Transaction transaction, List<Category> categories)
        {
            InitializeComponent();
            _transactionService = transactionService;
            _transaction = transaction;
            _categories = categories;

            // Підключаємо категорії до ComboBox
            CmbCategory.ItemsSource = _categories;

            // Заповнюємо поля
            TxtAmount.Text = _transaction.Amount.ToString();
            CmbType.SelectedIndex = _transaction.Type == Domain.Enums.TransactionType.Income ? 0 : 1;
            CmbCategory.SelectedValue = _transaction.CategoryId; // обирає поточну категорію
            DpDate.SelectedDate = _transaction.Date;
            TxtDescription.Text = _transaction.Description;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _transaction.Amount = decimal.Parse(TxtAmount.Text);
                _transaction.Type = CmbType.SelectedIndex == 0
                    ? Domain.Enums.TransactionType.Income
                    : Domain.Enums.TransactionType.Expense;
                _transaction.CategoryId = (int)CmbCategory.SelectedValue;
                var selectedDate = DpDate.SelectedDate ?? DateTime.Now;
                _transaction.Date = DateTime.SpecifyKind(selectedDate, DateTimeKind.Utc);
                _transaction.Description = TxtDescription.Text;

                await _transactionService.UpdateAsync(_transaction);
                MessageBox.Show("Транзакцію успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}\n\nDetails: {ex.InnerException?.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
