using System;
using System.Windows;
using System.Windows.Controls;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;

namespace MoneyRules.UI.Windows
{
    public partial class AddTransactionWindow : Window
    {
        public Transaction? CreatedTransaction { get; private set; }

        public AddTransactionWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Поточний користувач
                if (!System.Windows.Application.Current.Properties.Contains("CurrentUser") ||
                    System.Windows.Application.Current.Properties["CurrentUser"] is not User currentUser)
                {
                    MessageBox.Show("Будь ласка, увійдіть у систему.");
                    return;
                }

                // Перевірка суми
                if (!decimal.TryParse(AmountTextBox.Text, out var amount))
                {
                    MessageBox.Show("Некоректна сума. Введіть число.");
                    return;
                }

                // Дата UTC
                var selectedDate = DatePicker.SelectedDate ?? DateTime.Now;
                var utcDate = DateTime.SpecifyKind(selectedDate, DateTimeKind.Utc);

                // Тип транзакції
                var transactionType = TypeComboBox.SelectedIndex == 0 ? TransactionType.Income : TransactionType.Expense;
                var categoryType = TypeComboBox.SelectedIndex == 0 ? CategoryType.Category1 : CategoryType.Category2;

                // Створюємо транзакцію
                var transaction = new Transaction
                {
                    UserId = currentUser.UserId,
                    Type = transactionType,
                    Category = new Category
                    {
                        Name = CategoryTextBox.Text,
                        Type = categoryType,
                        UserId = currentUser.UserId
                    },
                    Amount = amount,
                    Date = utcDate,
                    Description = DescriptionTextBox.Text
                };

                // Зберігаємо в БД
                using var context = new AppDbContext();
                context.Transactions.Add(transaction);
                context.SaveChanges();

                MessageBox.Show("Транзакція успішно додана!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка при збереженні транзакції:\n{ex.Message}\n\n" +
                    $"{ex.InnerException?.Message}",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
