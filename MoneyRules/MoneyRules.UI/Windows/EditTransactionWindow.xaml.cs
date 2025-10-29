using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

            // Заповнення полів
            TxtAmount.Text = _transaction.Amount.ToString();
            CmbType.SelectedIndex = _transaction.Type == TransactionType.Income ? 0 : 1;
            TxtCategory.Text = _transaction.Category?.Name ?? "";
            DpDate.SelectedDate = _transaction.Date;
            TxtDescription.Text = _transaction.Description;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- Зчитування даних ---
                _transaction.Amount = decimal.Parse(TxtAmount.Text);
                _transaction.Type = CmbType.SelectedIndex == 0
                    ? TransactionType.Income
                    : TransactionType.Expense;

                // --- Обробка категорії ---
                string enteredCategory = TxtCategory.Text.Trim();
                if (string.IsNullOrWhiteSpace(enteredCategory))
                {
                    MessageBox.Show("Будь ласка, введіть назву категорії.",
                        "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Шукаємо, чи існує така категорія
                var existingCategory = _categories
                    .FirstOrDefault(c => string.Equals(c.Name, enteredCategory, StringComparison.OrdinalIgnoreCase));

                Category categoryToUse;

                if (existingCategory == null)
                {
                    // Якщо нова — створюємо
                    categoryToUse = new Category
                    {
                        Name = enteredCategory,
                        UserId = _transaction.UserId
                    };

                    categoryToUse = await _transactionService.CreateCategoryAsync(categoryToUse);
                }
                else
                {
                    categoryToUse = existingCategory;
                }

                _transaction.CategoryId = categoryToUse.CategoryId;

                // --- Інші дані ---
                _transaction.Date = DpDate.SelectedDate ?? DateTime.Now;
                _transaction.Description = TxtDescription.Text;

                await _transactionService.UpdateAsync(_transaction);

                MessageBox.Show("Транзакцію успішно оновлено!",
                    "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
