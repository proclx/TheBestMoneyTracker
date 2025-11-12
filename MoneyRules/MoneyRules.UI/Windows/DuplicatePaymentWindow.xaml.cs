using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.UI.Utils; // Наша фабрика
using System.Windows.Controls;
using MoneyRules.Domain.Enums; // Для TransactionType

namespace MoneyRules.UI.Windows
{
    public partial class DuplicatePaymentWindow : Window
    {
        private User? _currentUser;
        private Transaction? _selectedTransaction; // Зберігаємо обрану транзакцію

        public DuplicatePaymentWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Properties.Contains("CurrentUser") ||
                System.Windows.Application.Current.Properties["CurrentUser"] is not User user)
            {
                MessageBox.Show("Будь ласка, увійдіть у систему.");
                this.Close();
                return;
            }
            _currentUser = user;
            LoadTransactions();
        }

        // Завантажуємо всі транзакції користувача
        private void LoadTransactions()
        {
            try
            {
                using var context = DbContextFactory.Create();
                var transactions = context.Transactions
                    .Include(t => t.Category) 
                    .Where(t => t.UserId == _currentUser!.UserId)
                    .OrderByDescending(t => t.Date) // Недавні - зверху
                    .ToList();

                transactionsDataGrid.ItemsSource = transactions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження транзакцій: {ex.Message}");
            }
        }

        // Коли користувач обирає транзакцію, заповнюємо поле "Сума"
        private void TransactionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTransaction = transactionsDataGrid.SelectedItem as Transaction;
            if (_selectedTransaction != null)
            {
                // Заповнюємо поле сумою, як ви просили
                AmountTextBox.Text = _selectedTransaction.Amount.ToString("N2");
            }
        }

        // Зберігаємо дублікат
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTransaction == null)
            {
                // --- ЗМІНА ТУТ (1/2): 'MessageBoxWarning' замінено на 'MessageBoxImage.Warning' ---
                MessageBox.Show("Будь ласка, оберіть транзакцію зі списку.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(AmountTextBox.Text, out var newAmount))
            {
                // --- ЗМІНА ТУТ (2/2): 'MessageBoxError' замінено на 'MessageBoxImage.Error' ---
                MessageBox.Show("Некоректна сума. Введіть число.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Створюємо абсолютно новий об'єкт
                var newTransaction = new Transaction
                {
                    UserId = _selectedTransaction.UserId,
                    CategoryId = _selectedTransaction.CategoryId, // Та сама категорія
                    Type = _selectedTransaction.Type,         // Той самий тип
                    Description = _selectedTransaction.Description, // Той самий опис
                    Amount = newAmount,                       // Нова сума
                    Date = DateTime.UtcNow                    // Нова (поточна) дата
                };

                using var context = DbContextFactory.Create();
                context.Transactions.Add(newTransaction);
                context.SaveChanges();

                MessageBox.Show("Дублікат успішно створено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження дублікату: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}