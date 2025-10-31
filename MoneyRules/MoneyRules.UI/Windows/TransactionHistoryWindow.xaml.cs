using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MoneyRules.UI.Windows
{
    public partial class TransactionHistoryWindow : Window
    {
        private readonly ITransactionService _transactionService;
        private readonly int _currentUserId;
        private Transaction? _selectedTransaction;
        private bool _isFiltering = false;

        public TransactionHistoryWindow(ITransactionService transactionService, int currentUserId)
        {
            InitializeComponent();
            _transactionService = transactionService;
            _currentUserId = currentUserId;
            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            await LoadCategoriesAsync();
            LoadTransactionTypes();
            await ApplyFilterAsync(); // початкове завантаження
        }

        // ✅ Категорії без дублікатів
        private async Task LoadCategoriesAsync()
        {
            var transactions = await _transactionService.GetTransactionsAsync(new TransactionFilter
            {
                UserId = _currentUserId
            });

            var distinctCategories = transactions
                .Where(t => t.Category != null)
                .Select(t => t.Category)
                .GroupBy(c => c.CategoryId)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            distinctCategories.Insert(0, new Category { CategoryId = 0, Name = "Усі категорії" });

            CategoryComboBox.ItemsSource = distinctCategories;
            CategoryComboBox.DisplayMemberPath = "Name";
            CategoryComboBox.SelectedValuePath = "CategoryId";
            CategoryComboBox.SelectedIndex = 0;
        }

        // ✅ Типи транзакцій без дублювань
        private void LoadTransactionTypes()
        {
            var types = Enum.GetValues(typeof(TransactionType))
                            .Cast<TransactionType>()
                            .ToList();

            TypeComboBox.ItemsSource = new object[] { "Усі типи" }
                .Concat(types.Cast<object>())
                .ToList();

            TypeComboBox.SelectedIndex = 0;
        }

        // ✅ Тільки кнопка "Застосувати" викликає фільтр
        private async void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            await ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            if (_isFiltering)
                return;

            _isFiltering = true;

            try
            {
                var filter = new TransactionFilter
                {
                    UserId = _currentUserId,
                    FromDate = FromDatePicker.SelectedDate,
                    ToDate = ToDatePicker.SelectedDate
                };

                if (CategoryComboBox.SelectedItem is Category category && category.CategoryId != 0)
                    filter.CategoryId = category.CategoryId;

                if (TypeComboBox.SelectedItem is TransactionType selectedType)
                    filter.Type = selectedType;

                var transactions = await _transactionService.GetTransactionsAsync(filter);

                TransactionGrid.ItemsSource = null;
                TransactionGrid.ItemsSource = transactions.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні даних: {ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isFiltering = false;
            }
        }

        private async void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            CategoryComboBox.SelectedIndex = 0;
            TypeComboBox.SelectedIndex = 0;
            await ApplyFilterAsync();
        }

        private void TransactionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTransaction = TransactionGrid.SelectedItem as Transaction;
        }

        private async void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTransaction == null)
            {
                MessageBox.Show("Виберіть транзакцію для редагування.",
                    "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var categories = await _transactionService.GetUserCategoriesAsync(_currentUserId);
            var editWindow = new EditTransactionWindow(_transactionService, _selectedTransaction, categories);
            editWindow.ShowDialog();

            await LoadCategoriesAsync();
            await ApplyFilterAsync();
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
                    MessageBox.Show("Транзакцію видалено.", "Успіх",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await ApplyFilterAsync();
                }
                else
                {
                    MessageBox.Show("Транзакцію не знайдено.", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
