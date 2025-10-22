using System;
using System.Windows;
using MoneyRules.Application.Services;
using MoneyRules.Infrastructure.Persistence;

namespace MoneyRules.UI.Windows
{
    public partial class SetLimitWindow : Window
    {
        private readonly ExpenseLimitService _service;

        public SetLimitWindow(AppDbContext context)
        {
            InitializeComponent();
            _service = new ExpenseLimitService(context);
        }

        private async void SaveLimit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(AmountBox.Text, out var amount))
                {
                    MessageBox.Show("Некоректна сума!");
                    return;
                }

                if (!int.TryParse(MonthBox.Text, out var month) || month < 1 || month > 12)
                {
                    MessageBox.Show("Некоректний місяць!");
                    return;
                }

                if (!int.TryParse(YearBox.Text, out var year) || year < 2000)
                {
                    MessageBox.Show("Некоректний рік!");
                    return;
                }

                // Під час тесту можна використати тимчасовий користувач:
                Guid userId = Guid.NewGuid();

                await _service.SetLimitAsync(userId, amount, year, month);

                ResultText.Text = $"✅ Ліміт {amount} грн на {month}.{year} встановлено успішно!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
